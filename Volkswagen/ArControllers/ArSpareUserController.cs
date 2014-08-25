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
    public class ArSpareUserController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArSpareUser/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<ArSpareUserModels> list = db.ArSpareUsers.Where("1 = 1");
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
                return View(await db.ArSpareUsers.ToListAsync());
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

            IQueryable<ArSpareUserModels> list = getQuery();

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

        private IQueryable<ArSpareUserModels> getQuery()
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ArSpareUserModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArSpareUserModels).GetProperty(field));
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
            var e = db.ArSpareUsers;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(ArSpareUserModels) }, Expression.Constant(e), pred);

            IQueryable<ArSpareUserModels> list = db.ArSpareUsers.AsQueryable().Provider.CreateQuery<ArSpareUserModels>(expr);

            return list;
        }

        private List<ArSpareUserModels> getSelected(IQueryable<ArSpareUserModels> l)
        {
            List<ArSpareUserModels> list = new List<ArSpareUserModels>();
            List<ArSpareUserModels> list_origin = l.ToList();
            foreach (ArSpareUserModels e in list_origin)
            {
                if (Request.Form["Checked" + e.UserID] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /ArSpareUser/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareUserModels arspareusermodels = await db.ArSpareUsers.FindAsync(id);
            if (arspareusermodels == null)
            {
                return HttpNotFound();
            }
            return View(arspareusermodels);
        }

        // GET: /ArSpareUser/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareUserModels arspareusermodels = await db.ArSpareUsers.FindAsync(id);
            if (arspareusermodels == null)
            {
                return HttpNotFound();
            }
            return View(arspareusermodels);
        }

        // POST: /ArSpareUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ArSpareUserModels arspareusermodels = await db.ArSpareUsers.FindAsync(id);
            db.ArSpareUsers.Remove(arspareusermodels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /ArSpareUser/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<ArSpareUserModels> l = getQuery();
            List<ArSpareUserModels> list = getSelected(l);
            foreach (ArSpareUserModels e in list)
            {
                db.ArSpareUsers.Remove(e);
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

        // GET: /ArSpareUser/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<ArSpareUserModels> list = db.ArSpareUsers.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "备件物流编号",
        "备件名称",
        "备件型号",
        "入库数量",
        "出库数量",
        "领用人员",
        "领用时间",
        "是几使用设备",
        "最后修改时间",
        "修改人",
        "创建时间",
        "创建人",
                "操作类型",
                "操作时间"};
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.InValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OutValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.User);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.UseTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ActualUse);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.UserID);
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
            return File(fileStream, "application/ms-excel", "备件领用记录 - 历史记录.xls");
        }
    }
}
