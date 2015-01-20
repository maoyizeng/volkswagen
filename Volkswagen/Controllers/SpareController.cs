using MvcContrib.Pagination;
using MvcContrib.Sorting;
using MvcContrib.UI.Grid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using Volkswagen.DAL;
using Volkswagen.Models;

namespace Volkswagen.Controllers
{
    public class SpareController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Spare/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<SpareModels> list = getQuery(false);
            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    list = list.OrderBy(model.Column + " desc");
                }
                else
                {
                    list = list.OrderBy(model.Column + " asc");
                }
            }
            
            return View(list.ToList().AsPagination(page ?? 1, 100));
        }

        [HttpPost]
        public async Task<ActionResult> Index(int? page, string selected_item)
        {
            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<SpareModels> list = getQuery();

            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    list = list.OrderBy(model.Column + " desc");
                }
                else
                {
                    list = list.OrderBy(model.Column + " asc");
                }
            }

            return View(list.ToList().AsPagination(page ?? 1, 100));
        }

        private IQueryable<SpareModels> getQuery(bool post = true)
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(SpareModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++)
            {
                string field = (post ? Request.Form["field" + n] : Request["field" + n]);
                ViewData["field" + n] = field;
                string op = (post ? Request.Form["op" + n] : Request["op" + n]);
                ViewData["op" + n] = op;
                string operand = (post ? Request.Form["operand" + n] : Request["operand" + n]);
                ViewData["operand" + n] = operand;

                if (string.IsNullOrEmpty(field)) break;
                if (string.IsNullOrEmpty(operand)) continue;

                //p.[filedn]
                Expression left = Expression.Property(param, typeof(SpareModels).GetProperty(field));
                //[operandn]
                Expression right = Expression.Constant(operand);
                Expression result;

                switch (field)
                {
                    case "KeyPart":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(SpareModels.KeyPartType), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "ChangeTime":
                    case "CreateTime":
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "PresentValue":
                    case "SafeValue":
                    case "DCMinValue":
                    case "DCMaxValue":
                        right = Expression.Constant(int.Parse(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    default:
                    break;
                }

                switch (op)
                {
                    case "0":
                        result = Expression.Equal(left, right);
                        break;
                    case "1":
                        result = Expression.GreaterThan(left, right);
                        break;
                    case "2":
                        result = Expression.LessThan(left, right);
                        break;
                    case "3":
                        result = Expression.GreaterThanOrEqual(left, right);
                        break;
                    case "4":
                        result = Expression.LessThanOrEqual(left, right);
                        break;
                    case "5":
                        result = Expression.NotEqual(left, right);
                        break;
                    case "6": //Contain
                        result = Expression.Call(left, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), right);
                        break;
                    default:
                        result = Expression.Equal(left, right);
                        break;
                }
                filter = Expression.And(filter, result);
            }

            // p => p.[filedn] [opn] [operandn] && ...
            Expression pred = Expression.Lambda(filter, param);

            // where(p => p.[filedn] [opn] [operandn] && ...)
            var e = db.Spares;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(SpareModels) }, Expression.Constant(e), pred);

            IQueryable<SpareModels> list = db.Spares.AsQueryable().Provider.CreateQuery<SpareModels>(expr);

            return list;
        }

        private List<SpareModels> getSelected(IQueryable<SpareModels> l)
        {
            List<SpareModels> list = new List<SpareModels>();
            List<SpareModels> list_origin = l.ToList();
            foreach (SpareModels e in list_origin)
            {
                var ss = Request.Form["Checked" + e.SpareID];
                if ((!string.IsNullOrEmpty(ss)) && (ss != "false"))
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /Spare/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareModels sparemodels = await db.Spares.FindAsync(id);
            if (sparemodels == null)
            {
                return HttpNotFound();
            }
            return View(sparemodels);
        }

        // GET: /Spare/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            return View();
        }

        // POST: /Spare/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="SpareID,SpareDes,Type,Picture1,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,File,ChangeTime,Changer,CreateTime,Creator")] SpareModels sparemodels)
        {
            if (db.Equipments.Find(sparemodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(sparemodels);
            }
            if (db.Spares.Find(sparemodels.SpareID) != null)
            {
                ViewData["valid"] = "exist_key";
                return View(sparemodels);
            }
            if (ModelState.IsValid)
            {
                sparemodels.Changer = User.Identity.Name;
                sparemodels.Creator = User.Identity.Name;
                sparemodels.CreateTime = DateTime.Now;
                sparemodels.ChangeTime = DateTime.Now;
                db.Spares.Add(sparemodels);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ArSpareModels ar = new ArSpareModels(sparemodels);
                    ar.Operator = ArEquipmentModels.OperatorType.创建;
                    db.ArSpares.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", sparemodels.EquipmentID);
            return View(sparemodels);
        }

        // GET: /Spare/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareModels sparemodels = await db.Spares.FindAsync(id);
            if (sparemodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", sparemodels.EquipmentID);
            return View(sparemodels);
        }

        // POST: /Spare/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="SpareID,SpareDes,Type,Picture1,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,File,ChangeTime,Changer,CreateTime,Creator")] SpareModels sparemodels)
        {
            if (db.Equipments.Find(sparemodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(sparemodels);
            }
            if (ModelState.IsValid)
            {
                var toUpdate = db.Spares.Find(sparemodels.SpareID);

                sparemodels.Changer = User.Identity.Name;
                sparemodels.ChangeTime = DateTime.Now;
                sparemodels.Creator = toUpdate.Creator;
                sparemodels.CreateTime = toUpdate.CreateTime;

                db.Entry(toUpdate).State = EntityState.Detached;
                db.Entry(sparemodels).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArSpareModels ar = new ArSpareModels(toUpdate);
                    ar.Operator = ArEquipmentModels.OperatorType.修改;
                    db.ArSpares.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", sparemodels.EquipmentID);
            return View(sparemodels);
        }

        // POST: /Spare/EditMultiple/
        //[HttpPost]
        public async Task<ActionResult> EditMultiple()
        {
            IQueryable<SpareModels> l = getQuery();
            List<SpareModels> list = getSelected(l);
            if (ViewData["list"] == null) ViewData["list"] = list;
            //string key = list.First().SpareID;
            //return RedirectToAction("Edit", new { id = key });
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");

            return View("ChangeMultiple", new SpareModels());
        }

        // POST: /Spare/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "SpareID,SpareDes,Type,Picture1,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,File,ChangeTime,Changer,CreateTime,Creator")] SpareModels sparemodels)
        {
            if ((!string.IsNullOrEmpty(sparemodels.EquipmentID)) && (db.Equipments.Find(sparemodels.EquipmentID) == null))
            {
                ViewData["valid"] = "no_foreign";
                return View(sparemodels);
            }

            bool changed = false;
            List<SpareModels> l = new List<SpareModels>();
            for (int i = 0; ; i++)
            {
                string id = Request.Form["item" + i];
                if (Request.Form["item" + i] == null) break;
                SpareModels e = db.Spares.Find(id);
                l.Add(e);
                ArSpareModels ar = new ArSpareModels(e);
                if (sparemodels.SpareDes != null && ModelState.IsValidField("SpareDes")) e.SpareDes = sparemodels.SpareDes;
                if (sparemodels.Type != null && ModelState.IsValidField("Type")) e.Type = sparemodels.Type;
                if (sparemodels.Mark != null && ModelState.IsValidField("Mark")) e.Mark = sparemodels.Mark;
                if (sparemodels.PresentValue != null && ModelState.IsValidField("PresentValue")) e.PresentValue = sparemodels.PresentValue;
                if (sparemodels.SafeValue != null && ModelState.IsValidField("SafeValue")) e.SafeValue = sparemodels.SafeValue;
                if (sparemodels.DCMinValue != null && ModelState.IsValidField("DCMinValue")) e.DCMinValue = sparemodels.DCMinValue;
                if (sparemodels.DCMaxValue != null && ModelState.IsValidField("DCMaxValue")) e.DCMaxValue = sparemodels.DCMaxValue;
                if (sparemodels.Property != null && ModelState.IsValidField("Property")) e.Property = sparemodels.Property;
                if (sparemodels.EquipmentID != null && ModelState.IsValidField("EquipmentID")) e.EquipmentID = sparemodels.EquipmentID;
                if (sparemodels.Producer != null && ModelState.IsValidField("Producer")) e.Producer = sparemodels.Producer;
                if (sparemodels.OrderNumber != null && ModelState.IsValidField("OrderNumber")) e.OrderNumber = sparemodels.OrderNumber;
                if (sparemodels.KeyPart != null && ModelState.IsValidField("KeyPart")) e.KeyPart = sparemodels.KeyPart;
                if (sparemodels.Remark != null && ModelState.IsValidField("Remark")) e.Remark = sparemodels.Remark;

                if (db.Entry(e).State == EntityState.Modified)
                {
                    e.Changer = User.Identity.Name;
                    e.ChangeTime = DateTime.Now;
                    int x = await db.SaveChangesAsync();
                    if (x != 0)
                    {
                        changed = true;
                        ar.Operator = ArEquipmentModels.OperatorType.修改;
                        db.ArSpares.Add(ar);
                        await db.SaveChangesAsync();
                    }
                }
            }
            if (changed)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewData["list"] = l;
                ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
                return View(new SpareModels());
            }
        }


        // GET: /Spare/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareModels sparemodels = await db.Spares.FindAsync(id);
            if (sparemodels == null)
            {
                return HttpNotFound();
            }
            return View(sparemodels);
        }

        // POST: /Spare/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            SpareModels toDelete = await db.Spares.FindAsync(id);
            ArSpareModels ar = new ArSpareModels(toDelete);
            db.Spares.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.删除;
                db.ArSpares.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /Spare/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple(int? page, string selected_item)
        {
            IQueryable<SpareModels> l = getQuery();
            List<SpareModels> list = getSelected(l);
            foreach (SpareModels e in list)
            {
                ArSpareModels ar = new ArSpareModels(e);
                db.Spares.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ar.Operator = ArEquipmentModels.OperatorType.删除;
                    db.ArSpares.Add(ar);
                    await db.SaveChangesAsync();
                }
            }

            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;
            l = getQuery();

            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    l = l.OrderBy(model.Column + " desc");
                }
                else
                {
                    l = l.OrderBy(model.Column + " asc");
                }
            }

            return View("Index", l.ToList().AsPagination(page ?? 1, 100));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public ActionResult FileRemove()
        {
            string key = Request.Form["key2"];

            SpareModels e = db.Spares.Find(key);
            if (e == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string origin_photo = e.Picture1;
            string new_photo = "";
            string[] sArray = Regex.Split(origin_photo, "[$]");

            for (int i = 1; i < sArray.Length; i++)
            {
                if (Request.Form["photo_" + i] != "on")
                {
                    new_photo += "$" + sArray[i];
                }
            }

            ArSpareModels ar = new ArSpareModels(e);
            // TODO - check e;
            e.Changer = User.Identity.Name;
            e.ChangeTime = DateTime.Now;
            e.Picture1 = new_photo;
            int x = db.SaveChanges();

            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.修改;
                db.ArSpares.Add(ar);
                db.SaveChanges();
            }
            return RedirectToAction("Edit", new { id = key });
        }

        // POST: /Spare/FileUpload
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult FileUpload(HttpPostedFileBase[] photos)
        {
            string key = Request.Form["key"];
            string fullname = "";
            string directory = AppDomain.CurrentDomain.BaseDirectory + @"img\spare\";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            foreach (HttpPostedFileBase file in photos)
            {
                if (file != null)
                {
                    string filename = DateTime.Now.ToString("yy-MM-dd HH-mm-ss") + " - " + Path.GetFileName(file.FileName);
                    string filePath = Path.Combine(directory, filename); 
                    file.SaveAs(filePath);
                    fullname += "$" + filename;
                }
            }

            SpareModels e = db.Spares.Find(key);
            ArSpareModels ar = new ArSpareModels(e);

            e.Changer = User.Identity.Name;
            e.ChangeTime = DateTime.Now;
            e.Picture1 += fullname;
            int x = db.SaveChanges();

            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.修改;
                db.ArSpares.Add(ar);
                db.SaveChanges();
            }
            return RedirectToAction("Edit", new { id = key });

        }

        public ActionResult ExportExcel()
        {
            //var sbHtml = new StringBuilder();
            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;

            var l = getQuery();

            // 排序
            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    l = l.OrderBy(model.Column + " desc");
                }
                else
                {
                    l = l.OrderBy(model.Column + " asc");
                }
            }

            var list = l.ToList();

            /*sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");*/
            var lstTitle = new List<string> { 
                "设备物流编号",
        "备件名称",
        "备件型号",
        "仓位号",
        "当前库存",
        "安全库存",
        "东昌最小库存",
        "东昌最大库存",
        "所属设备",
        "设备编号",
        "设备供货商",
        "订货号",
        "设备关键属性",
        "最后修改时间",
        "修改人",
        "创建时间",
        "创建人",
        "备注"
            };
            /*
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            foreach (var i in list)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.SpareID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.SpareDes);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Type);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Mark);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.PresentValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.SafeValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.DCMinValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.DCMaxValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Property);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Producer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OrderNumber);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.KeyPart);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ChangeTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Changer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CreateTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Creator);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Remark);
                if (Request.Form["export_picture"] != "false")
                {
                    sbHtml.AppendFormat("<td>{0}</td>", i.Picture1);
                }
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "备件库存.xls");*/

            // 用随机数构造一个新的文件夹
            Random random = new Random();
            int ranfolder = random.Next();
            string folder = AppDomain.CurrentDomain.BaseDirectory + @"files\tmp\" + ranfolder;
            Directory.CreateDirectory(folder);

            // 打开excel和工作簿
            Application app = new Application();
            Workbooks wbks = app.Workbooks;

            // 打开模版文件 之后会另存
            _Workbook wbk = wbks.Add();
            Sheets shs = wbk.Sheets;
            _Worksheet sh = shs.Item[1];

            int j = 1;
            foreach (var item in lstTitle)
            {
                sh.Cells[1, j] = item;
                j++;
            }

            j = 1;
            foreach (var i in list)
            {
                j++;
                sh.Cells[j, 1] = i.SpareID;
                sh.Cells[j, 2] = i.SpareDes;
                sh.Cells[j, 3] = i.Type;
                sh.Cells[j, 4] = i.Mark;
                sh.Cells[j, 5] = i.PresentValue;
                sh.Cells[j, 6] = i.SafeValue;
                sh.Cells[j, 7] = i.DCMinValue;
                sh.Cells[j, 8] = i.DCMaxValue;
                sh.Cells[j, 9] = i.Property;
                sh.Cells[j, 10] = i.EquipmentID;
                sh.Cells[j, 11] = i.Producer;
                sh.Cells[j, 12] = i.OrderNumber;
                sh.Cells[j, 13] = i.KeyPart;
                sh.Cells[j, 14] = i.ChangeTime;
                sh.Cells[j, 15] = i.Changer;
                sh.Cells[j, 16] = i.CreateTime;
                sh.Cells[j, 17] = i.Creator;
                sh.Cells[j, 18] = i.Remark;
                if (Request.Form["export_picture"] != "false") //如果需要输出图片
                {
                    string origin_photo = i.Picture1;
                    string[] sArray = Regex.Split(origin_photo, "[$]");

                    float pleft = Convert.ToSingle(sh.Cells[j, 19].Left); //图片左侧锚点
                    foreach (var photo in sArray)
                    {
                        float ptop = Convert.ToSingle(sh.Cells[j, 19].Top); //图片上方锚点
                        float pheight = (float)(5 / 0.035); //高度 0.035为1磅 这个可自调
                        float pwidth = (float)(5 / 0.035);  //宽度
                        if (!string.IsNullOrEmpty(photo))
                        {
                            sh.Shapes.AddPicture(AppDomain.CurrentDomain.BaseDirectory + @"img\spare\" + photo, MsoTriState.msoFalse, MsoTriState.msoTrue, pleft, ptop, pwidth, pheight);
                        }
                        pleft += (float)(5 / 0.035); //右移一张图片的位置
                    }
                    sh.Rows[j].RowHeight = 5 / 0.035; //含图片的行高
                }
                sh.Columns[14].ColumnWidth = 16; //时间输出的列宽
                sh.Columns[16].ColumnWidth = 16;
            }
            wbk.SaveAs(folder + @"\备件库存.xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, XlSaveConflictResolution.xlLocalSessionChanges, Missing.Value, Missing.Value, Missing.Value, Missing.Value);

            wbk.Close(null, null, null);
            wbks.Close();
            app.Quit();

            //释放掉多余的excel进程
            System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
            app = null;

            return File(folder + @"\备件库存.xls", "application/ms-excel", "备件库存.xls");
        }
    }
}
