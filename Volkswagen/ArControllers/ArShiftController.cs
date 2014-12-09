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
using System.Linq.Expressions;
using MvcContrib.UI.Grid;
using System.IO;
using MvcContrib.Sorting;
using MvcContrib.Pagination;
using System.Text;
using System.Linq.Dynamic;

namespace Volkswagen.ArControllers
{
    [Authorize(Roles = "Admin")]
    public class ArShiftController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArShift/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<ArShiftModels> list = getQuery(false);
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

            IQueryable<ArShiftModels> list = getQuery();

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

        private IQueryable<ArShiftModels> getQuery(bool post = true)
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ArShiftModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArShiftModels).GetProperty(field));
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
                    case "OperateTime":
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "RecordID":
                        right = Expression.Constant(int.Parse(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "Operator":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(ArEquipmentModels.OperatorType), operand)));
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
            var e = db.ArShifts;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(ArShiftModels) }, Expression.Constant(e), pred);

            IQueryable<ArShiftModels> list = db.ArShifts.AsQueryable().Provider.CreateQuery<ArShiftModels>(expr);

            return list;
        }

        private List<ArShiftModels> getSelected(IQueryable<ArShiftModels> l)
        {
            List<ArShiftModels> list = new List<ArShiftModels>();
            List<ArShiftModels> list_origin = l.ToList();
            foreach (ArShiftModels e in list_origin)
            {
                if (Request.Form["Checked" + e.ShiftID + e.OperateTime.ToBinary()] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /ArShift/Details/5
        public async Task<ActionResult> Details(int id)
        {
            ArShiftModels arShiftmodels = await db.ArShifts.FindAsync(id);
            if (arShiftmodels == null)
            {
                return HttpNotFound();
            }

            ShiftModels e = await db.Shifts.FindAsync(arShiftmodels.ShiftID);
            ViewData["origin"] = e;
            return View(arShiftmodels);
        }

        // GET: /ArShift/Rollback/5
        public async Task<ActionResult> Rollback(int id)
        {
            ArShiftModels a = await db.ArShifts.FindAsync(id);
            if (a == null)
            {
                return HttpNotFound();
            }
            ShiftModels origin = await db.Shifts.FindAsync(a.ShiftID);

            ArEquipmentModels.OperatorType change;

            switch (a.Operator)
            {
                case ArEquipmentModels.OperatorType.创建:
                    if (origin == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "表中已不存在此记录");
                    }
                    db.Shifts.Remove(origin);
                    change = ArEquipmentModels.OperatorType.删除;
                    break;
                case ArEquipmentModels.OperatorType.修改:
                    if (origin == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "表中已不存在此记录");
                    }
                    origin.upcast(a);
                    change = ArEquipmentModels.OperatorType.修改;
                    break;
                case ArEquipmentModels.OperatorType.删除:
                    if (origin != null)
                    {
                        change = ArEquipmentModels.OperatorType.修改;
                    }
                    else
                    {
                        change = ArEquipmentModels.OperatorType.创建;
                        db.Shifts.Add(origin);
                    }
                    origin = new ShiftModels();
                    origin.upcast(a);
                    origin.Creator = User.Identity.Name;
                    origin.CreateTime = DateTime.Now;
                    break;
                default:
                    if (origin == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    change = ArEquipmentModels.OperatorType.修改;
                    break;
            }

            origin.Changer = User.Identity.Name;
            origin.ChangeTime = DateTime.Now;

            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ArShiftModels ar = new ArShiftModels(origin);
                ar.Operator = change;
                db.ArShifts.Add(ar);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GET: /ArShift/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArShiftModels arshiftmodels = await db.ArShifts.FindAsync(id);
            if (arshiftmodels == null)
            {
                return HttpNotFound();
            }
            return View(arshiftmodels);
        }

        // POST: /ArShift/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ArShiftModels arshiftmodels = await db.ArShifts.FindAsync(id);
            db.ArShifts.Remove(arshiftmodels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /ArShift/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<ArShiftModels> l = getQuery();
            List<ArShiftModels> list = getSelected(l);
            foreach (ArShiftModels e in list)
            {
                db.ArShifts.Remove(e);
                await db.SaveChangesAsync();
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

        // GET: /ArShift/ExportExcel
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
                "历史编号",
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
        "创建人",
                "操作类型",
                "操作时间" };
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            foreach (var i in list)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.RecordID);
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Operator);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OperateTime);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "交班记录 - 历史记录.xls");
        }
    }
}
