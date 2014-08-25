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
    public class ArSpareOrderController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArSpareOrder/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<ArSpareOrderModels> list = db.ArSpareOrders.Where("1 = 1");
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
                return View(await db.ArSpareOrders.ToListAsync());
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

            IQueryable<ArSpareOrderModels> list = getQuery();

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

        private IQueryable<ArSpareOrderModels> getQuery()
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ArSpareOrderModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArSpareOrderModels).GetProperty(field));
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
            var e = db.ArSpareOrders;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(ArSpareOrderModels) }, Expression.Constant(e), pred);

            IQueryable<ArSpareOrderModels> list = db.ArSpareOrders.AsQueryable().Provider.CreateQuery<ArSpareOrderModels>(expr);

            return list;
        }

        private List<ArSpareOrderModels> getSelected(IQueryable<ArSpareOrderModels> l)
        {
            List<ArSpareOrderModels> list = new List<ArSpareOrderModels>();
            List<ArSpareOrderModels> list_origin = l.ToList();
            foreach (ArSpareOrderModels e in list_origin)
            {
                if (Request.Form["Checked" + e.OrderID + e.OperateTime.ToBinary()] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /ArSpareOrder/Details/5
        public async Task<ActionResult> Details(int? id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareOrderModels arspareordermodels = await db.ArSpareOrders.FindAsync(id, op, new DateTime(opt));
            if (arspareordermodels == null)
            {
                return HttpNotFound();
            }
            return View(arspareordermodels);
        }

        // GET: /ArSpareOrder/Rollback/5
        public async Task<ActionResult> Rollback(int? id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareOrderModels a = await db.ArSpareOrders.FindAsync(id, op, new DateTime(opt));
            if (a == null)
            {
                return HttpNotFound();
            }
            SpareOrderModels origin = await db.SpareOrders.FindAsync(id);
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
                origin = new SpareOrderModels();
                origin.upcast(a);
                origin.Changer = User.Identity.Name;
                origin.Creator = User.Identity.Name;
                origin.CreateTime = DateTime.Now;
                origin.ChangeTime = DateTime.Now;
                change = "Create";
                db.SpareOrders.Add(origin);
            }

            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ArSpareOrderModels ar = new ArSpareOrderModels(origin);
                ar.Operator = change;
                db.ArSpareOrders.Add(ar);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GET: /ArSpareOrder/Delete/5
        public async Task<ActionResult> Delete(int? id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareOrderModels arspareordermodels = await db.ArSpareOrders.FindAsync(id, op, new DateTime(opt));
            if (arspareordermodels == null)
            {
                return HttpNotFound();
            }
            return View(arspareordermodels);
        }

        // POST: /ArSpareOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id, string op, long opt)
        {
            ArSpareOrderModels arspareordermodels = await db.ArSpareOrders.FindAsync(id, op, new DateTime(opt));
            db.ArSpareOrders.Remove(arspareordermodels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /ArSpareOrder/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<ArSpareOrderModels> l = getQuery();
            List<ArSpareOrderModels> list = getSelected(l);
            foreach (ArSpareOrderModels e in list)
            {
                db.ArSpareOrders.Remove(e);
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

        // GET: /ArSpareOrder/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<ArSpareOrderModels> list = db.ArSpareOrders.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "备件物流编号",
        "备件名称",
        "备件型号",
        "订购数量",
        "备件制造商",
        "订货号",
        "所属设备",
        "设备编号",
        "设备商",
        "产品编号",
        "订购人",
        "订购日期",
        "单价金额",
        "总金额",
        "状态",
        "采购形式",
        "编号",
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OrderValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Producer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OrderNum);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Property);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Maker);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MakerNum);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Orderman);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.UseTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.UnitPrice);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.TotalPrice);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Status);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Mode);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OrderID);
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
            return File(fileStream, "application/ms-excel", "备件订购信息 - 历史记录.xls");
        }
    }
}
