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
    public class ArSpareController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArSpare/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<ArSpareModels> list = db.ArSpares.Where("1 = 1");
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
                return View(await db.ArSpares.ToListAsync());
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

            IQueryable<ArSpareModels> list = getQuery();

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

        private IQueryable<ArSpareModels> getQuery()
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ArSpareModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArSpareModels).GetProperty(field));
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
            var e = db.ArSpares;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(ArSpareModels) }, Expression.Constant(e), pred);

            IQueryable<ArSpareModels> list = db.ArSpares.AsQueryable().Provider.CreateQuery<ArSpareModels>(expr);

            return list;
        }

        private List<ArSpareModels> getSelected(IQueryable<ArSpareModels> l)
        {
            List<ArSpareModels> list = new List<ArSpareModels>();
            List<ArSpareModels> list_origin = l.ToList();
            foreach (ArSpareModels e in list_origin)
            {
                if (Request.Form["Checked" + e.SpareID] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /ArSpare/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareModels arsparemodels = await db.ArSpares.FindAsync(id);
            if (arsparemodels == null)
            {
                return HttpNotFound();
            }
            return View(arsparemodels);
        }

        // GET: /ArSpare/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareModels arsparemodels = await db.ArSpares.FindAsync(id);
            if (arsparemodels == null)
            {
                return HttpNotFound();
            }
            return View(arsparemodels);
        }

        // POST: /ArSpare/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<ArSpareModels> l = getQuery();
            List<ArSpareModels> list = getSelected(l);
            foreach (ArSpareModels e in list)
            {
                db.ArSpares.Remove(e);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /ArSpare/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            ArSpareModels arsparemodels = await db.ArSpares.FindAsync(id);
            db.ArSpares.Remove(arsparemodels);
            await db.SaveChangesAsync();
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

        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<ArSpareModels> list = db.ArSpares.ToList();

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
        "备注",
                "操作类型",
                "操作时间"
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Operator);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OperateTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Remark);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "备件库存 - 历史记录.xls");
        }
    }
}
