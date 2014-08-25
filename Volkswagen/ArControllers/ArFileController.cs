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
    public class ArFileController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArFile/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<ArFileModels> list = db.ArFiles.Where("1 = 1");
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
                return View(await db.ArFiles.ToListAsync());
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

            IQueryable<ArFileModels> list = getQuery();

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

        private IQueryable<ArFileModels> getQuery()
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ArFileModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArFileModels).GetProperty(field));
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
            var e = db.ArFiles;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(ArFileModels) }, Expression.Constant(e), pred);

            IQueryable<ArFileModels> list = db.ArFiles.AsQueryable().Provider.CreateQuery<ArFileModels>(expr);

            return list;
        }

        private List<ArFileModels> getSelected(IQueryable<ArFileModels> l)
        {
            List<ArFileModels> list = new List<ArFileModels>();
            List<ArFileModels> list_origin = l.ToList();
            foreach (ArFileModels e in list_origin)
            {
                if (Request.Form["Checked" + e.FileName] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /ArFile/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArFileModels arfilemodels = await db.ArFiles.FindAsync(id);
            if (arfilemodels == null)
            {
                return HttpNotFound();
            }
            return View(arfilemodels);
        }

        // GET: /ArFile/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArFileModels arfilemodels = await db.ArFiles.FindAsync(id);
            if (arfilemodels == null)
            {
                return HttpNotFound();
            }
            return View(arfilemodels);
        }

        // POST: /ArFile/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            ArFileModels arfilemodels = await db.ArFiles.FindAsync(id);
            db.ArFiles.Remove(arfilemodels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /ArFile/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<ArFileModels> l = getQuery();
            List<ArFileModels> list = getSelected(l);
            foreach (ArFileModels e in list)
            {
                db.ArFiles.Remove(e);
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

        // GET: /ArFile/ExportExcel
        public ArFileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<ArFileModels> list = db.ArFiles.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "文件名",
                "类别",
                "设备编号",
                "设备名称",
                "文件负责人",
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

            string format = "<td style='font-size: 12px;height:20px;'>{0}</td>";
            foreach (var i in list)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat(format, i.FileName);
                sbHtml.AppendFormat(format, i.Class);
                sbHtml.AppendFormat(format, i.EquipmentID);
                sbHtml.AppendFormat(format, i.EquipDes);
                sbHtml.AppendFormat(format, i.Charger);
                // TODO - file?
                sbHtml.AppendFormat(format, i.ChangeTime);
                sbHtml.AppendFormat(format, i.Changer);
                sbHtml.AppendFormat(format, i.CreateTime);
                sbHtml.AppendFormat(format, i.Creator);
                sbHtml.AppendFormat(format, i.Operator);
                sbHtml.AppendFormat(format, i.OperateTime);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "文件库 - 历史记录.xls");
        }
    }
}
