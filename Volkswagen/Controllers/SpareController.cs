using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Volkswagen.Models;
using Volkswagen.DAL;
using MvcContrib.UI.Grid;
using System.Linq.Expressions;
using System.IO;
using System.Linq.Dynamic;
using MvcContrib.Sorting;
using MvcContrib.Pagination;
using System.Text;

namespace Volkswagen.Controllers
{
    public class SpareController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Spare/
        public async Task<ActionResult> Index(int? page, GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<SpareModels> list = db.Spares.Where("1 = 1");
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
            else
            {
                return View(db.Spares.ToList().AsPagination(page ?? 1, 200));
            }
            return View(list.ToList().AsPagination(page ?? 1, 200));
        }

        [HttpPost]
        public async Task<ActionResult> Index(int? page)
        {
            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;

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

            return View(list.ToList().AsPagination(page ?? 1, 200));
        }

        private IQueryable<SpareModels> getQuery()
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(SpareModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++)
            {
                string field = Request.Form["field" + n];
                ViewData["field" + n] = field;
                string op = Request.Form["op" + n];
                ViewData["op" + n] = op;
                string operand = Request.Form["operand" + n];
                ViewData["operand" + n] = operand;

                if (string.IsNullOrEmpty(field)) break;
                if (string.IsNullOrEmpty(operand)) continue;

                //p.[filedn]
                Expression left = Expression.Property(param, typeof(SpareModels).GetProperty(field));
                //[operandn]
                Expression right = Expression.Constant(operand);
                Expression result;

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
                if (Request.Form["Checked" + e.SpareID] != "false")
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
        public async Task<ActionResult> Create([Bind(Include="SpareID,SpareDes,Type,Picture1,Picture2,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,File,ChangeTime,Changer,CreateTime,Creator")] SpareModels sparemodels)
        {
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
                    ar.Operator = "Create";
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
        public async Task<ActionResult> Edit([Bind(Include="SpareID,SpareDes,Type,Picture1,Picture2,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,File,ChangeTime,Changer,CreateTime,Creator")] SpareModels sparemodels)
        {
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
                    ar.Operator = "Update";
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
            
            return RedirectToAction("ChangeMultiple", new { Sparemodels = new SpareModels() });
        }

        // POST: /Spare/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "SpareID,SpareDes,Type,Picture1,Picture2,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,File,ChangeTime,Changer,CreateTime,Creator")] SpareModels sparemodels)
        {
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
                        ar.Operator = "Update";
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
            db.Spares.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ArSpareModels ar = new ArSpareModels(toDelete);
                ar.Operator = "Delete";
                db.ArSpares.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /Spare/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<SpareModels> l = getQuery();
            List<SpareModels> list = getSelected(l);
            foreach (SpareModels e in list)
            {
                db.Spares.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ArSpareModels ar = new ArSpareModels(e);
                    ar.Operator = "Delete";
                    db.ArSpares.Add(ar);
                    await db.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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

            foreach (HttpPostedFileBase file in photos)
            {
                if (file != null)
                {
                    string filePath = Path.Combine((AppDomain.CurrentDomain.BaseDirectory + @"img\spare\"), Path.GetFileName(file.FileName));
                    file.SaveAs(filePath);
                    fullname += "$" + file.FileName;
                }
            }

            SpareModels e = db.Spares.Find(key);
            // TODO - check e;
            e.Picture1 = fullname;
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = key });

        }

        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<SpareModels> list = db.Spares.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Picture1);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Picture2);
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
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "备件库存.xls");
        }
    }
}
