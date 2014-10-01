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
    public class ArRepairController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArRepair/
        public async Task<ActionResult> Index(int? page, GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<ArRepairModels> list = db.ArRepairs.Where("1 = 1");
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
                return View(db.ArRepairs.ToList().AsPagination(page ?? 1, 200));
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

            IQueryable<ArRepairModels> list = getQuery();

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

        private IQueryable<ArRepairModels> getQuery()
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ArRepairModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArRepairModels).GetProperty(field));
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
            var e = db.ArRepairs;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(ArRepairModels) }, Expression.Constant(e), pred);

            IQueryable<ArRepairModels> list = db.ArRepairs.AsQueryable().Provider.CreateQuery<ArRepairModels>(expr);

            return list;
        }

        private List<ArRepairModels> getSelected(IQueryable<ArRepairModels> l)
        {
            List<ArRepairModels> list = new List<ArRepairModels>();
            List<ArRepairModels> list_origin = l.ToList();
            foreach (ArRepairModels e in list_origin)
            {
                if (Request.Form["Checked" + e.SheetID + e.OperateTime.ToBinary()] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /ArRepair/Details/5
        public async Task<ActionResult> Details(string id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArRepairModels arrepairmodels = await db.ArRepairs.FindAsync(id, op, new DateTime(opt));
            if (arrepairmodels == null)
            {
                return HttpNotFound();
            }
            return View(arrepairmodels);
        }

        // GET: /ArRepair/Rollback/5
        public async Task<ActionResult> Rollback(string id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArRepairModels a = await db.ArRepairs.FindAsync(id, op, new DateTime(opt));
            if (a == null)
            {
                return HttpNotFound();
            }
            RepairModels origin = await db.Repairs.FindAsync(id);
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
                origin = new RepairModels();
                origin.upcast(a);
                origin.Changer = User.Identity.Name;
                origin.Creator = User.Identity.Name;
                origin.CreateTime = DateTime.Now;
                origin.ChangeTime = DateTime.Now;
                change = "Create";
                db.Repairs.Add(origin);
            }

            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ArRepairModels ar = new ArRepairModels(origin);
                ar.Operator = change;
                db.ArRepairs.Add(ar);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GET: /ArRepair/Delete/5
        public async Task<ActionResult> Delete(string id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArRepairModels arrepairmodels = await db.ArRepairs.FindAsync(id, op, new DateTime(opt));
            if (arrepairmodels == null)
            {
                return HttpNotFound();
            }
            return View(arrepairmodels);
        }

        // POST: /ArRepair/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id, string op, long opt)
        {
            ArRepairModels arrepairmodels = await db.ArRepairs.FindAsync(id, op, new DateTime(opt));
            db.ArRepairs.Remove(arrepairmodels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /ArRepair/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<ArRepairModels> l = getQuery();
            List<ArRepairModels> list = getSelected(l);
            foreach (ArRepairModels e in list)
            {
                db.ArRepairs.Remove(e);
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

        // GET: /ArRepair/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<ArRepairModels> list = db.ArRepairs.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "原单据编号",
        "保修单号",
        "设备名称",
        "设备编号",
        "保修时刻",
        "修复时刻",
        "维修耗时",
        "班次",
        "车间生产线",
        "工段",
        "故障现象",
        "维修人",
        "故障原因和维修内容",
        "故障类别",
        "已修复否",
        "遗留问题",
        "验收人",
        "备注",
        "停机时间",
        "维修次数",
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.SheetID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.RepairID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipDes);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.StartTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.FinishTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.RepairTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Class);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Line);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Section);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.FaultView);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Repairman);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Description);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.FaultType);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Result);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Problem);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Checker);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Remark);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.StopTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.RepairNum);
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
            return File(fileStream, "application/ms-excel", "设备报修单 - 历史记录.xls");
        }
    }
}
