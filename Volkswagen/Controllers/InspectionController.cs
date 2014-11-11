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

namespace Volkswagen.Controllers
{
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
                    case "PlanID":
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
                if (Request.Form["Checked" + e.InspectionId] != "false")
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
            return View();
        }

        // POST: /Inspection/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "InspectionId,PlanID,EquipmentID,EquipDes,Class,Part,Position,Content,Period,Caution,Remark,ChangeTime,Changer,CreateTime,Creator")] InspectionModels inspectionmodels)
        {
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
                    ArInspectionModels ar = new ArInspectionModels(inspectionmodels);
                    ar.Operator = "Create";
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
                    ar.Operator = "Update";
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
            return RedirectToAction("ChangeMultiple", new { Inspectionmodels = new InspectionModels() });
        }

        // POST: /Inspection/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "InspectionId,PlanID,EquipmentID,EquipDes,Class,Part,Position,Content,Period,Caution,Remark,ChangeTime,Changer,CreateTime,Creator")] InspectionModels inspectionmodels)
        {
            bool changed = false;
            List<InspectionModels> l = new List<InspectionModels>();
            for (int i = 0; ; i++)
            {
                string id = Request.Form["item" + i];
                if (Request.Form["item" + i] == null) break;
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
                if (inspectionmodels.Remark != null && ModelState.IsValidField("Remark")) e.Remark = inspectionmodels.Remark;

                if (db.Entry(e).State == EntityState.Modified)
                {
                    e.Changer = User.Identity.Name;
                    e.ChangeTime = DateTime.Now;
                    int x = await db.SaveChangesAsync();
                    if (x != 0)
                    {
                        changed = true;
                        ar.Operator = "Update";
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
            db.Inspections.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ArInspectionModels ar = new ArInspectionModels(toDelete);
                ar.Operator = "Delete";
                db.ArInspections.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /Inspection/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<InspectionModels> l = getQuery();
            List<InspectionModels> list = getSelected(l);
            foreach (InspectionModels e in list)
            {
                db.Inspections.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ArInspectionModels ar = new ArInspectionModels(e);
                    ar.Operator = "Delete";
                    db.ArInspections.Add(ar);
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

        // GET: /Inspection/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<InspectionModels> list = db.Inspections.ToList();

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

        public FileResult ExportYearPlan()
        {
            IQueryable<InspectionModels> l = getQuery();
            var data = l.Where("1 = 1").ToList();

            Application app = new Application();
            Workbooks wbks = app.Workbooks;
            _Workbook wbk = wbks.Add(true);
            Sheets shs = wbk.Sheets;
            _Worksheet sh = shs.Add();

            // http://www.cnblogs.com/wang_yb/articles/1750419.html
            // TODO - generate excel data


            //屏蔽掉系统跳出的Alert
            app.AlertBeforeOverwriting = false;

            //保存到指定目录
            wbk.SaveAs("file/inspection_plan/年度设备保养计划.xls", Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);

            wbk.Close(null, null, null);
            wbks.Close();
            app.Quit();

            //释放掉多余的excel进程
            System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
            app = null;
            return File("file/inspection_plan/年度设备保养计划.xls", "application/ms-excel", "年度设备保养计划.xls");
        }
    }
}
