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
    [UserAuthorized]
    public class ShiftController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Shift/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<ShiftModels> list = getQuery(false);
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

            IQueryable<ShiftModels> list = getQuery();

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

        private IQueryable<ShiftModels> getQuery(bool post = true)
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ShiftModels), "p");
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
                Expression left = Expression.Property(param, typeof(ShiftModels).GetProperty(field));
                //[operandn]
                Expression right = Expression.Constant(operand);
                Expression result;

                switch (field)
                {
                    case "Class":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(RepairModels.ClassType), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "Line":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(EquipmentModels.WSNames), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "ChangeTime":
                    case "CreateTime":
                    case "ShiftDate":
                    case "ShiftTime":
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "ShiftID":
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
            var e = db.Shifts;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(ShiftModels) }, Expression.Constant(e), pred);

            IQueryable<ShiftModels> list = db.Shifts.AsQueryable().Provider.CreateQuery<ShiftModels>(expr);

            return list;
        }

        private List<ShiftModels> getSelected(IQueryable<ShiftModels> l)
        {
            List<ShiftModels> list = new List<ShiftModels>();
            List<ShiftModels> list_origin = l.ToList();
            foreach (ShiftModels e in list_origin)
            {
                var ss = Request.Form["Checked" + e.ShiftID];
                if ((!string.IsNullOrEmpty(ss)) && (ss != "false"))
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /Shift/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Shift/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="ShiftDate,ShiftTime,Class,Line,Charger,Record,Urgency,Remark,ChangeTime,Changer,CreateTime,Creator")] ShiftModels shiftmodels)
        {
            if (ModelState.IsValid)
            {
                shiftmodels.Changer = User.Identity.Name;
                shiftmodels.Creator = User.Identity.Name;
                shiftmodels.CreateTime = DateTime.Now;
                shiftmodels.ChangeTime = DateTime.Now;
                db.Shifts.Add(shiftmodels);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    var new_ins = db.Shifts.OrderByDescending(p => p.ShiftID).First();
                    ArShiftModels ar = new ArShiftModels(new_ins);
                    ar.Operator = ArEquipmentModels.OperatorType.创建;
                    db.ArShifts.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }

            return View(shiftmodels);
        }

        // GET: /Shift/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShiftModels shiftmodels = await db.Shifts.FindAsync(id);
            if (shiftmodels == null)
            {
                return HttpNotFound();
            }
            return View(shiftmodels);
        }

        // POST: /Shift/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="ShiftID,ShiftDate,ShiftTime,Class,Line,Charger,Record,Urgency,Remark,ChangeTime,Changer,CreateTime,Creator")] ShiftModels shiftmodels)
        {
            if (ModelState.IsValid)
            {
                var toUpdate = db.Shifts.Find(shiftmodels.ShiftID);

                shiftmodels.Changer = User.Identity.Name;
                shiftmodels.ChangeTime = DateTime.Now;
                shiftmodels.Creator = toUpdate.Creator;
                shiftmodels.CreateTime = toUpdate.CreateTime;

                db.Entry(toUpdate).State = EntityState.Detached;
                db.Entry(shiftmodels).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArShiftModels ar = new ArShiftModels(toUpdate);
                    ar.Operator = ArEquipmentModels.OperatorType.修改;
                    db.ArShifts.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            return View(shiftmodels);
        }

        // POST: /Shift/EditMultiple/
        //[HttpPost]
        public async Task<ActionResult> EditMultiple()
        {
            IQueryable<ShiftModels> l = getQuery();
            List<ShiftModels> list = getSelected(l);
            if (ViewData["list"] == null) ViewData["list"] = list;
            //string key = list.First().ShiftID;
            //return RedirectToAction("Edit", new { id = key });
            return View("ChangeMultiple", new ShiftModels());
        }

        // POST: /Shift/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "ShiftID,ShiftDate,ShiftTime,Class,Line,Charger,Record,Urgency,Remark,ChangeTime,Changer,CreateTime,Creator")] ShiftModels shiftmodels)
        {
            bool changed = false;
            List<ShiftModels> l = new List<ShiftModels>();
            for (int i = 0; ; i++)
            {
                if (Request.Form["item" + i] == null) break;
                int id = int.Parse(Request.Form["item" + i]);
                ShiftModels e = db.Shifts.Find(id);
                l.Add(e);
                ArShiftModels ar = new ArShiftModels(e);
                if (shiftmodels.ShiftDate != null && ModelState.IsValidField("ShiftDate")) e.ShiftDate = shiftmodels.ShiftDate;
                if (shiftmodels.ShiftTime != null && ModelState.IsValidField("ShiftTime")) e.ShiftTime = shiftmodels.ShiftTime;
                if (shiftmodels.Class != null && ModelState.IsValidField("Class")) e.Class = shiftmodels.Class;
                if (shiftmodels.Line != null && ModelState.IsValidField("Line")) e.Line = shiftmodels.Line;
                if (shiftmodels.Charger != null && ModelState.IsValidField("Charger")) e.Charger = shiftmodels.Charger;
                if (shiftmodels.Record != null && ModelState.IsValidField("Record")) e.Record = shiftmodels.Record;
                if (shiftmodels.Urgency != null && ModelState.IsValidField("Urgency")) e.Urgency = shiftmodels.Urgency;
                if (shiftmodels.Remark != null && ModelState.IsValidField("Remark")) e.Remark = shiftmodels.Remark;

                if (db.Entry(e).State == EntityState.Modified)
                {
                    e.Changer = User.Identity.Name;
                    e.ChangeTime = DateTime.Now;
                    int x = await db.SaveChangesAsync();
                    if (x != 0)
                    {
                        changed = true;
                        ar.Operator = ArEquipmentModels.OperatorType.修改;
                        db.ArShifts.Add(ar);
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
                return View(new ShiftModels());
            }
        }


        // GET: /Shift/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShiftModels shiftmodels = await db.Shifts.FindAsync(id);
            if (shiftmodels == null)
            {
                return HttpNotFound();
            }
            return View(shiftmodels);
        }

        // POST: /Shift/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ShiftModels toDelete = await db.Shifts.FindAsync(id);
            ArShiftModels ar = new ArShiftModels(toDelete);
            db.Shifts.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.删除;
                db.ArShifts.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /Shift/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple(int? page, string selected_item)
        {
            IQueryable<ShiftModels> l = getQuery();
            List<ShiftModels> list = getSelected(l);
            foreach (ShiftModels e in list)
            {
                ArShiftModels ar = new ArShiftModels(e);
                db.Shifts.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ar.Operator = ArEquipmentModels.OperatorType.删除;
                    db.ArShifts.Add(ar);
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

        // GET: /Shift/ExportExcel
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
                "记录编号",
        "交班日期",
        "时间",
        "班次",
        "车间生产线",
        "负责人",
        "情况记录",
        "紧急事件",
        "备注",
        "最后修改时间",
        "修改人",
        "创建时间",
        "创建人" };
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            foreach (var i in list)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ShiftID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ShiftDate);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ShiftTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Class);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Line);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Charger);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Record);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Urgency);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Remark);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ChangeTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Changer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CreateTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Creator);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "交班记录.xls");
        }
    }
}
