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
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;

namespace Volkswagen.Controllers
{
    [UserAuthorized]
    
    public class InspectionController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Inspection/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<InspectionModels> list = getQuery(false);
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
            string newone = Request.Form["Direction"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<InspectionModels> list = getQuery(true);

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

        private IQueryable<InspectionModels> getQuery(bool post = true)
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(InspectionModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++)
            {
                string field = (post? Request.Form["field" + n] : Request["field" + n]);
                ViewData["field" + n] = field;
                string op = (post ? Request.Form["op" + n] : Request["op" + n]);
                ViewData["op" + n] = op;
                string operand = (post ? Request.Form["operand" + n] : Request["operand" + n]);
                ViewData["operand" + n] = operand;

                if (string.IsNullOrEmpty(field)) break;
                if (string.IsNullOrEmpty(operand)) continue;

                Expression left;
                if (field == "Person")
                {
                    //p.Equipments.Person
                    left = Expression.Property(Expression.Property(param, typeof(InspectionModels).GetProperty("Equipments")), typeof(EquipmentModels).GetProperty(field));
                } else if (field == "WSArea")
                {
                    left = Expression.Property(Expression.Property(param, typeof(InspectionModels).GetProperty("Equipments")), typeof(EquipmentModels).GetProperty(field));
                }
                else
                {
                    //p.[filedn]
                    left = Expression.Property(param, typeof(InspectionModels).GetProperty(field));
                }
                //[operandn]
                Expression right = Expression.Constant(operand);
                Expression result;

                /*if (field == "Class")
                {
                    right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(InspectionModels.InspectionClass), operand)));
                    right = Expression.Convert(right, left.Type);
                }
                else if ((field == "ChangeTime") || (field == "CreateTime"))
                {
                    right = Expression.Constant(Convert.ToDateTime(operand));
                    right = Expression.Convert(right, left.Type);
                }*/

                switch (field)
                {
                    case "Class":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(InspectionModels.InspectionClass), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "ChangeTime":
                    case "CreateTime":
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "InspectionId":
                        right = Expression.Constant(int.Parse(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "WSArea":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(EquipmentModels.WSNames), operand)));
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
            var e = db.Inspections;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(InspectionModels) }, Expression.Constant(e), pred);

            IQueryable<InspectionModels> list = db.Inspections.AsQueryable().Provider.CreateQuery<InspectionModels>(expr);

            return list;
        }

        private List<InspectionModels> getSelected(IQueryable<InspectionModels> l)
        {
            List<InspectionModels> list = new List<InspectionModels>();
            List<InspectionModels> list_origin = l.ToList();
            foreach (InspectionModels e in list_origin)
            {
                var ss = Request.Form["Checked" + e.InspectionId];
                if ((!string.IsNullOrEmpty(ss)) && (ss != "false"))
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /Inspection/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InspectionModels inspectionmodels = await db.Inspections.FindAsync(id);
            if (inspectionmodels == null)
            {
                return HttpNotFound();
            }
            return View(inspectionmodels);
        }

        // GET: /Inspection/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
            ViewBag.Person = new SelectList(db.Equipments.Select(e => e.Person).Distinct(), "Person");
            return View();
        }

        // POST: /Inspection/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "PlanID,EquipmentID,EquipDes,Class,Part,Position,Content,Period,Caution,Remark,ChangeTime,Changer,CreateTime,Creator")] InspectionModels inspectionmodels)
        {
            if (db.Equipments.Find(inspectionmodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(inspectionmodels);
            }
            if (ModelState.IsValid)
            {
                inspectionmodels.Changer = User.Identity.Name;
                inspectionmodels.Creator = User.Identity.Name;
                inspectionmodels.CreateTime = DateTime.Now;
                inspectionmodels.ChangeTime = DateTime.Now;
                db.Inspections.Add(inspectionmodels);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    var new_ins = db.Inspections.OrderByDescending(p => p.InspectionId).First();
                    ArInspectionModels ar = new ArInspectionModels(new_ins);
                    ar.Operator = ArEquipmentModels.OperatorType.创建;
                    db.ArInspections.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", inspectionmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", inspectionmodels.EquipDes);
            return View(inspectionmodels);
        }

        // GET: /Inspection/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InspectionModels inspectionmodels = await db.Inspections.FindAsync(id);
            if (inspectionmodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", inspectionmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", inspectionmodels.EquipDes); 
            return View(inspectionmodels);
        }

        // POST: /Inspection/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "InspectionId,PlanID,EquipmentID,EquipDes,Class,Part,Position,Content,Period,Caution,Remark,ChangeTime,Changer,CreateTime,Creator")] InspectionModels inspectionmodels)
        {
            if (db.Equipments.Find(inspectionmodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(inspectionmodels);
            }
            if (ModelState.IsValid)
            {
                var toUpdate = db.Inspections.Find(inspectionmodels.InspectionId);

                inspectionmodels.Changer = User.Identity.Name;
                inspectionmodels.ChangeTime = DateTime.Now;
                inspectionmodels.Creator = toUpdate.Creator;
                inspectionmodels.CreateTime = toUpdate.CreateTime;

                db.Entry(toUpdate).State = EntityState.Detached;
                db.Entry(inspectionmodels).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArInspectionModels ar = new ArInspectionModels(toUpdate);
                    ar.Operator = ArEquipmentModels.OperatorType.修改;
                    db.ArInspections.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", inspectionmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", inspectionmodels.EquipDes);
            return View(inspectionmodels);
        }

        // POST: /Inspection/EditMultiple/
        //[HttpPost]
        public async Task<ActionResult> EditMultiple()
        {
            IQueryable<InspectionModels> l = getQuery();
            List<InspectionModels> list = getSelected(l);
            if (ViewData["list"] == null) ViewData["list"] = list;
            //string key = list.First().InspectionID;
            //return RedirectToAction("Edit", new { id = key });
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
            return View("ChangeMultiple", new InspectionModels());
        }

        // POST: /Inspection/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "InspectionId,PlanID,EquipmentID,EquipDes,Class,Part,Position,Content,Period,Caution,Remark,ChangeTime,Changer,CreateTime,Creator")] InspectionModels inspectionmodels)
        {
            if ((!string.IsNullOrEmpty(inspectionmodels.EquipmentID)) && (db.Equipments.Find(inspectionmodels.EquipmentID) == null))
            {
                ViewData["valid"] = "no_foreign";
                return View(inspectionmodels);
            }
            bool changed = false;
            List<InspectionModels> l = new List<InspectionModels>();
            for (int i = 0; ; i++)
            {
                if (Request.Form["item" + i] == null) break;
                int id = int.Parse(Request.Form["item" + i]);                
                InspectionModels e = db.Inspections.Find(id);
                l.Add(e);
                ArInspectionModels ar = new ArInspectionModels(e);
                if (inspectionmodels.EquipmentID != null && ModelState.IsValidField("EquipmentID")) e.EquipmentID = inspectionmodels.EquipmentID;
                if (inspectionmodels.EquipDes != null && ModelState.IsValidField("EquipDes")) e.EquipDes = inspectionmodels.EquipDes;
                if (inspectionmodels.Class != null && ModelState.IsValidField("Class")) e.Class = inspectionmodels.Class;
                if (inspectionmodels.Part != null && ModelState.IsValidField("Part")) e.Part = inspectionmodels.Part;
                if (inspectionmodels.Position != null && ModelState.IsValidField("Position")) e.Position = inspectionmodels.Position;
                if (inspectionmodels.EquipDes != null && ModelState.IsValidField("EquipDes")) e.EquipDes = inspectionmodels.EquipDes;
                if (inspectionmodels.Content != null && ModelState.IsValidField("Content")) e.Content = inspectionmodels.Content;
                if (inspectionmodels.Period != null && ModelState.IsValidField("Period")) e.Period = inspectionmodels.Period;
                if (inspectionmodels.Caution != null && ModelState.IsValidField("Caution")) e.Caution = inspectionmodels.Caution;
                if (inspectionmodels.PlanID != null && ModelState.IsValidField("PlanID")) e.PlanID = inspectionmodels.PlanID;
                if (inspectionmodels.Remark != null && ModelState.IsValidField("Remark")) e.Remark = inspectionmodels.Remark;

                if (db.Entry(e).State == EntityState.Modified)
                {
                    e.Changer = User.Identity.Name;
                    e.ChangeTime = DateTime.Now;
                    int x = await db.SaveChangesAsync();
                    if (x != 0)
                    {
                        changed = true;
                        ar.Operator = ArEquipmentModels.OperatorType.修改;
                        db.ArInspections.Add(ar);
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
                ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
                return View(new InspectionModels());
            }
        }

        // GET: /Inspection/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InspectionModels inspectionmodels = await db.Inspections.FindAsync(id);
            if (inspectionmodels == null)
            {
                return HttpNotFound();
            }
            return View(inspectionmodels);
        }

        // POST: /Inspection/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            InspectionModels toDelete = await db.Inspections.FindAsync(id);
            ArInspectionModels ar = new ArInspectionModels(toDelete);
            db.Inspections.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                try
                {
                    ar.Operator = ArEquipmentModels.OperatorType.删除;
                    db.ArInspections.Add(ar);
                    await db.SaveChangesAsync();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbex)
                {

                }
            }
            return RedirectToAction("Index");
        }

        // POST: /Inspection/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple(int? page, string selected_item)
        {
            IQueryable<InspectionModels> l = getQuery();
            List<InspectionModels> list = getSelected(l);
            foreach (InspectionModels e in list)
            {
                ArInspectionModels ar = new ArInspectionModels(e);
                db.Inspections.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ar.Operator = ArEquipmentModels.OperatorType.删除;
                    db.ArInspections.Add(ar);
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

        // GET: /Inspection/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
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

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "设备编号",
                "设备名称",
                "维护类别",
                "保养部件",
                "保养位置",
                "保养内容",
                "保养周期",
                "注意事项",
                "编号",
                "保养计划编号",                
                "最后修改时间",
                "修改人",
                "创建时间",
                "创建人",
                "备注"};
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            foreach (var i in list)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipDes);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Class);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Part);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Position);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Content);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Period);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Caution);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.InspectionId);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.PlanID);
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
            return File(fileStream, "application/ms-excel", "设备保养计划.xls");
        }

        [HttpPost]
        public ActionResult ExportYearPlan()
        {
            IQueryable<InspectionModels> l = getQuery();
            var data = l.ToList();
            int year = int.Parse(Request.Form["year_file"]);

            // 用随机数构造一个新的文件夹
            Random random = new Random();
            int ranfolder = random.Next();
            string folder = AppDomain.CurrentDomain.BaseDirectory + @"files\tmp\" + ranfolder;
            Directory.CreateDirectory(folder);            

            // 对每个设备进行逐个输出
            foreach (string equip_number in l.Select(p => p.EquipmentID).Distinct())
            {
                // 当前设备的所有保养计划
                var data_now = data.Where(p => p.EquipmentID == equip_number).ToList();

                // 设备的基本信息
                string equip_name = data_now.First().EquipDes;
                string equip_person = data_now.First().Equipments.Person;
                string equip_line = data_now.First().Equipments.WSArea.ToString();

                // 打开excel和工作簿
                Application app = new Application();
                Workbooks wbks = app.Workbooks;

                // 打开模版文件 之后会另存
                _Workbook wbk = wbks.Add(AppDomain.CurrentDomain.BaseDirectory + @"files\file_template\设备年度保养计划表.xls");
                Sheets shs = wbk.Sheets;
                //_Worksheet sh = shs.Add();

                // http://www.cnblogs.com/wang_yb/articles/1750419.html
                // 对每个sheet进行操作 最多15个
                int sheet = 1;
                for (sheet = 1; sheet <= 15; sheet++)
                {
                    bool finished = false; // flag 表示是不是输出完了所有的内容
                    
                    // 得到当前sheet对象
                    _Worksheet _wsh = (_Worksheet)shs.get_Item(sheet);

                    // 基本信息的输出
                    _wsh.Cells[2, 3] = year + "年度设备保养计划";
                    _wsh.Cells[4, 3] = "设备名称: " + equip_name;
                    _wsh.Cells[4, 15] = "设备技术员: " + equip_person;
                    _wsh.Cells[5, 3] = "设备编号: " + equip_number;
                    _wsh.Cells[5, 15] = "编制日期: " + DateTime.Now.Year + "." + DateTime.Now.Month;
                    _wsh.Cells[5, 25] = "车间: " + equip_line;

                    // 正文 最多15行
                    for (int i = 1; i <= 15; i++)
                    {
                        int line = i + 8;
                        InspectionModels current; 
                        if (data_now.Count == ((sheet - 1) * 15 + i - 1))
                        {
                            // 如果现在的总序号和要输出的个数相等 那么本设备的输出已经结束
                            finished = true;
                            break;
                        }
                        else
                        {
                            // 得到当前项
                            current = data_now.ElementAt((sheet - 1) * 15 + i - 1);
                        }

                        // 输出保养信息
                        _wsh.Cells[line, 1] = i;
                        _wsh.Cells[line, 2] = current.Part;
                        _wsh.Cells[line, 3] = current.Position;
                        _wsh.Cells[line, 4] = current.Content;

                        // 保养周期
                        string[] s_months = { };
                        if (current.Period != null)
                        {
                            // 将保养周期按","分割
                            s_months = Regex.Split(current.Period, ",", RegexOptions.IgnorePatternWhitespace);
                        }

                        // 填入x
                        foreach (string str in s_months)
                        {
                            if (str == "1-12")
                            {
                                // 如果只有一项1-12 那么12个月都要填 然后跳出
                                for (int m = 21; m <= 32; m++)
                                {
                                    _wsh.Cells[line, m] = "x";
                                }
                                break;
                            }
                            // 根据这是第几个月, 在那一栏内填上x
                            int mm = int.Parse(str);
                            _wsh.Cells[line, mm + 20] = "x";
                        }
                    }

                    // 日期
                    _wsh.Cells[33, 28] = DateTime.Now.Year + "年" + DateTime.Now.Month + "月至" + DateTime.Now.Year + "年12月";
                    if (finished)
                    {
                        break; //如果已经结束本设备 跳出
                    }

                }

                // 页码 当前/总页数
                for (int i = 1; i <= sheet; i++)
                {
                    ((_Worksheet)shs.get_Item(i)).Cells[35, 31] = i + "/" + sheet;
                }

                // 删除多余的sheet 要把删除时的alert先关掉
                app.DisplayAlerts = false;
                for (int i = 15; i > sheet; i--)
                {
                    ((_Worksheet)shs.get_Item(i)).Delete();
                }
                app.DisplayAlerts = true;
                
                int ran = random.Next();

                //保存到指定目录
                wbk.SaveAs(folder + @"\" + equip_name + ".xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, XlSaveConflictResolution.xlLocalSessionChanges, Missing.Value, Missing.Value, Missing.Value, Missing.Value);

                //if (Request.Form["print_year"] != "false")
                //{
                //    wbk.PrintOutEx(Type.Missing, sheet, Type.Missing, false, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                //}

                wbk.Close(null, null, null);
                wbks.Close();
                app.Quit();

                //释放掉多余的excel进程
                System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
                app = null;
            }

            // 压缩
            FastZip fz = new FastZip();
            fz.CreateZip(folder + @"\..\" + ranfolder + ".zip", folder, false, "");
            fz = null;

            // 返回
            return File(folder + @"\..\" + ranfolder + ".zip", "application/zip", year + "年度设备保养计划.zip");
        }

        [HttpPost]
        public ActionResult ExportMonthPlan()
        {
            IQueryable<InspectionModels> l = getQuery();
            var data = l.ToList();
            int year = int.Parse(Request.Form["month_file_year"]);
            int month = int.Parse(Request.Form["month_file_month"]);

            Random random = new Random();
            int ranfolder = random.Next();
            string folder = AppDomain.CurrentDomain.BaseDirectory + @"files\tmp\" + ranfolder;
            Directory.CreateDirectory(folder);

            // 打开excel
            Application app = new Application();
            Workbooks wbks = app.Workbooks;
            
            // 对每个设备逐个生成excel
            foreach (string equip_number in l.Select(p => p.EquipmentID).Distinct())
            {
                var data_now = data.Where(p => p.EquipmentID == equip_number).ToList();

                bool printed_something = false;

                string equip_name = data_now.First().EquipDes;
                string equip_person = data_now.First().Equipments.Person;
                string equip_line = data_now.First().Equipments.WSArea.ToString();
                
                // 打开模板 之后另存为新的工作本
                _Workbook wbk = wbks.Add(AppDomain.CurrentDomain.BaseDirectory + @"files\file_template\设备月度保养计划表.xls");
                Sheets shs = wbk.Sheets;
                //_Worksheet sh = shs.Add();

                // http://www.cnblogs.com/wang_yb/articles/1750419.html
                // 对15个sheet逐个操作
                int sheet = 1;
                int index = 0;
                for (sheet = 1; sheet <= 15; sheet++)
                {
                    bool finished = false; // flag 表示是不是输出完了所有的内容
                    _Worksheet _wsh = (_Worksheet)shs.get_Item(sheet);

                    // 基本信息
                    _wsh.Cells[2, 3] = year + "年" + month + "月设备维修保养记录卡";
                    _wsh.Cells[4, 3] = "设备名称: " + equip_name;
                    _wsh.Cells[4, 16] = "设备技术员: " + equip_person;
                    _wsh.Cells[5, 3] = "设备编号: " + equip_number;
                    _wsh.Cells[5, 16] = "编制日期: " + DateTime.Now.Year + "." + DateTime.Now.Month;
                    _wsh.Cells[5, 27] = "车间: " + equip_line;

                    // 输出正文, 一共15行 逐行输出
                    for (int i = 1; i <= 15; i++)
                    {
                        int line = i + 8;
                        InspectionModels current;

                        // 只有保养周期中有当月才输出
                        while (index < data_now.Count)
                        {
                            current = data_now.ElementAt(index);
                            bool to_print = false; // flag 表示要不要输出
                            if ((current.Period == null) || (current.Period.Contains("1-12")))
                            {
                                // 保养周期为空或为1-12 输出
                                to_print = true;
                                break;
                            }
                            else
                            {
                                string[] s_months = { };
                                if (current.Period != null)
                                {
                                    // 用","将保养周期的字符串分割
                                    s_months = Regex.Split(current.Period, ",", RegexOptions.IgnorePatternWhitespace);
                                }

                                // 如果分割出的某一项跟这个月匹配, 就要输出
                                foreach (string str in s_months)
                                {
                                    int mm = int.Parse(str);
                                    if (mm == month)
                                    {
                                        to_print = true;
                                        break;
                                    }
                                }
                            }
                            // 要输出 跳出循环继续
                            if (to_print)
                            {
                                printed_something = true;
                                break;
                            }
                            // 跳过此项 下一个循环, 查看下一个部件
                            index++;
                        }
                        
                        if (data_now.Count == index)
                        {
                            // 在找下一项的过程中发现所有项已遍历 结束本设备输出任务
                            finished = true;
                            break;
                        }
                        else
                        {
                            // 找到了要输出的部件 继续执行
                            current = data_now.ElementAt(index);
                        }

                        // 填写部件
                        _wsh.Cells[line, 1] = i;
                        _wsh.Cells[line, 2] = current.Part;
                        _wsh.Cells[line, 3] = current.Position;
                        _wsh.Cells[line, 4] = current.Content;
                        _wsh.Cells[line, 29] = month + "月底";

                        //下一个部件
                        index++;
                    }

                    // 所有项已遍历 结束本设备的输出任务
                    if (finished)
                    {
                        break;
                    }
                }

                // 给每个sheet加页标 当前/总页数
                for (int i = 1; i <= sheet; i++)
                {
                    ((_Worksheet)shs.get_Item(i)).Cells[28, 35] = i + "/" + sheet;
                }

                // 将多的sheet删除 将删除时的alert关掉
                app.DisplayAlerts = false;
                for (int i = 15; i > sheet; i--)
                {
                    ((_Worksheet)shs.get_Item(i)).Delete();
                }
                app.DisplayAlerts = true;

                int ran = random.Next();

                if (printed_something) { 
                    //保存到指定目录
                    wbk.SaveAs(folder + @"\" + equip_name + ".xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, XlSaveConflictResolution.xlLocalSessionChanges, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }

                //if (Request.Form["print_month"] != "false")
                //{
                //    wbk.PrintOutEx(Type.Missing, sheet, Type.Missing, false, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                //}

                wbk.Close(false, null, null);                

                // 结束 输出下一个设备的报表
            }

            wbks.Close();
            app.Quit();

            //释放掉多余的excel进程
            System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
            app = null;

            // 压缩
            FastZip fz = new FastZip();
            fz.CreateZip(folder + @"\..\" + ranfolder + ".zip", folder, false, "");
            fz = null;

            // 返回
            return File(folder + @"\..\" + ranfolder + ".zip", "application/zip", year + "年" + month + "月设备保养计划.zip");
        }

        [HttpPost]
        public async Task<ActionResult> ImportTable(HttpPostedFileBase[] tables)
        {
            string key = Request.Form["tables"];
            Random random = new Random();

            foreach (HttpPostedFileBase file in tables)
            {
                if (file != null)
                {
                    // 先保存好
                    int ran = random.Next();
                    string filePath = Path.Combine((AppDomain.CurrentDomain.BaseDirectory + @"files\tmp\"), ran + Path.GetFileName(file.FileName));
                    
                    file.SaveAs(filePath);

                    // 打开工作簿
                    Application app = new Application();
                    Workbooks wbks = app.Workbooks;
                    _Workbook wbk = wbks.Add(filePath);
                    Sheets shs = wbk.Sheets;
                    Range range = null;

                    // 遍历sheet
                    foreach (_Worksheet _wsh in shs)
                    {
                        range = _wsh.get_Range("C4", Missing.Value);
                        // 如果设备履历项没有值(仅有"设备履历:") 则sheet为空 结束此工作簿
                        if ((range.Value2 as string).Length < 6) {
                            break;
                        }
                        // 截取设备名称
                        string equip_name = (range.Value2 as string).Substring(5).Trim();

                        // 截取设备编号
                        range = _wsh.get_Range("C5", Missing.Value);
                        string equip_number = (range.Value2 as string).Substring(5).Trim();

                        // 找正文, 一共15行
                        for (int i = 0; i < 15; i++)
                        {
                            int line = i + 9;

                            // 如果序号为空 说明正文结束
                            range = _wsh.Range[_wsh.Cells[line, 1], _wsh.Cells[line, 1]];
                            if (!(range.Value2 is double))
                            {
                                break;
                            }

                            // 构造一个新的记录
                            InspectionModels im = new InspectionModels();
                            im.EquipmentID = equip_number;
                            im.EquipDes = equip_name;
                            range = _wsh.Range[_wsh.Cells[line, 2], _wsh.Cells[line, 2]];
                            im.Part = range.Value2 as string;
                            range = _wsh.Range[_wsh.Cells[line, 3], _wsh.Cells[line, 3]];
                            im.Position = range.Value2 as string;
                            range = _wsh.Range[_wsh.Cells[line, 4], _wsh.Cells[line, 4]];
                            im.Content = range.Value2 as string;

                            // 保养周期, 搜索12列, 将有x的月份拼成一个字符串
                            string period = "";
                            bool period_all = true; // flag 表示是否是1-12都有
                            for (int month = 1; month <= 12; month++)
                            {
                                range = _wsh.Range[_wsh.Cells[line, month + 20], _wsh.Cells[line, month + 20]];
                                string hasx = range.Value2 as string;
                                if ((hasx != null) && (hasx.Length != 0))
                                {
                                    if (period != "")
                                    {
                                        period += ",";
                                    }
                                    period += month;
                                }
                                else
                                {
                                    period_all = false;
                                }
                            }
                            if (period_all)
                            {
                                // 如果12个月都有, 放弃之前的字符串 改用"1-12"
                                period = "1-12";
                            }

                            im.Period = period;
                            //im.InspectionId = db.Inspections.Max(p => p.InspectionId) + 1;

                            // 必要的设置 并添加
                            im.Changer = User.Identity.Name;
                            im.Creator = User.Identity.Name;
                            im.CreateTime = DateTime.Now;
                            im.ChangeTime = DateTime.Now;
                            db.Inspections.Add(im);
                            int x = await db.SaveChangesAsync();
                            if (x != 0)
                            {
                                ArInspectionModels ar = new ArInspectionModels(db.Inspections.OrderByDescending(p => p.InspectionId).First());
                                ar.Operator = ArEquipmentModels.OperatorType.创建;
                                db.ArInspections.Add(ar);
                                await db.SaveChangesAsync();
                            }
                            else
                            {
                                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Input!");
                            }
                        }
                        
                    }

                    range = null;
                    wbk.Close(null, null, null);
                    wbks.Close();
                    app.Quit();

                    //释放掉多余的excel进程
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
                    app = null;
                }
            }
            return RedirectToAction("Index");
        }

        
    }
}
