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
    public class ArMaintainController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArMaintain/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<ArMaintainModels> list = db.ArMaintains.Where("1 = 1");
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
                return View(await db.ArMaintains.ToListAsync());
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

            IQueryable<ArMaintainModels> list = getQuery();

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

        private IQueryable<ArMaintainModels> getQuery()
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ArMaintainModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArMaintainModels).GetProperty(field));
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
            var e = db.ArMaintains;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(ArMaintainModels) }, Expression.Constant(e), pred);

            IQueryable<ArMaintainModels> list = db.ArMaintains.AsQueryable().Provider.CreateQuery<ArMaintainModels>(expr);

            return list;
        }

        private List<ArMaintainModels> getSelected(IQueryable<ArMaintainModels> l)
        {
            List<ArMaintainModels> list = new List<ArMaintainModels>();
            List<ArMaintainModels> list_origin = l.ToList();
            foreach (ArMaintainModels e in list_origin)
            {
                if (Request.Form["Checked" + e.MaintainId] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /ArMaintain/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArMaintainModels armaintainmodels = await db.ArMaintains.FindAsync(id);
            if (armaintainmodels == null)
            {
                return HttpNotFound();
            }
            return View(armaintainmodels);
        }        

        // GET: /ArMaintain/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArMaintainModels armaintainmodels = await db.ArMaintains.FindAsync(id);
            if (armaintainmodels == null)
            {
                return HttpNotFound();
            }
            return View(armaintainmodels);
        }

        // POST: /ArMaintain/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ArMaintainModels armaintainmodels = await db.ArMaintains.FindAsync(id);
            db.ArMaintains.Remove(armaintainmodels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /ArMaintain/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<ArMaintainModels> l = getQuery();
            List<ArMaintainModels> list = getSelected(l);
            foreach (ArMaintainModels e in list)
            {
                db.ArMaintains.Remove(e);
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

        // GET: /ArMaintain/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<ArMaintainModels> list = db.ArMaintains.ToList();

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
                "备注",
                "编号",
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipDes);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Line);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MType);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MPart);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Content);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Period);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MStartTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MEndTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ResponseClass);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckStatus);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipDetail);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckerType);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Checker);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Problem);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Mark);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Grade);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ProblemStatus);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckNum);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MaintainId);
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
            return File(fileStream, "application/ms-excel", "设备保养计划 - 历史记录.xls");
        }
    }
}
