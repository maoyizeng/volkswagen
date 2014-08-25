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
using System.Text;
using System.Linq.Dynamic;

namespace Volkswagen.ArControllers
{
    public class ArShiftController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArShift/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<ArShiftModels> list = db.ArShifts.Where("1 = 1");
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
                return View(await db.ArShifts.ToListAsync());
            }
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> Index()
        {
            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;

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

            return View(list);
        }

        private IQueryable<ArShiftModels> getQuery()
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ArShiftModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArShiftModels).GetProperty(field));
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
        public async Task<ActionResult> Details(string id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArShiftModels arshiftmodels = await db.ArShifts.FindAsync(id, op, new DateTime(opt));
            if (arshiftmodels == null)
            {
                return HttpNotFound();
            }
            return View(arshiftmodels);
        }

        // GET: /ArShift/Rollback/5
        public async Task<ActionResult> Rollback(string id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArShiftModels a = await db.ArShifts.FindAsync(id, op, new DateTime(opt));
            if (a == null)
            {
                return HttpNotFound();
            }
            ShiftModels origin = await db.Shifts.FindAsync(id);
            string change;
            if (origin != null)
            {
                origin.upcast(a);
                origin.Changer = User.Identity.Name;
                origin.ChangeTime = DateTime.Now;
                change = "Update";
            }
            else
            {
                origin = new ShiftModels();
                origin.upcast(a);
                origin.Changer = User.Identity.Name;
                origin.Creator = User.Identity.Name;
                origin.CreateTime = DateTime.Now;
                origin.ChangeTime = DateTime.Now;
                change = "Create";
                db.Shifts.Add(origin);
            }

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
        public async Task<ActionResult> Delete(string id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArShiftModels arshiftmodels = await db.ArShifts.FindAsync(id, op, new DateTime(opt));
            if (arshiftmodels == null)
            {
                return HttpNotFound();
            }
            return View(arshiftmodels);
        }

        // POST: /ArShift/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id, string op, long opt)
        {
            ArShiftModels arshiftmodels = await db.ArShifts.FindAsync(id, op, new DateTime(opt));
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
            List<ArShiftModels> list = db.ArShifts.ToList();

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
