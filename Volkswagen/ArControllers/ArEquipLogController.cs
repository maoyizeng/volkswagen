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
    public class ArEquipLogController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArEquipLog/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<ArEquipLogModels> list = getQuery(false);
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

            IQueryable<ArEquipLogModels> list = getQuery();

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

        private IQueryable<ArEquipLogModels> getQuery(bool post = true)
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ArEquipLogModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArEquipLogModels).GetProperty(field));
                //[operandn]
                Expression right = Expression.Constant(operand);
                Expression result;

                switch (field)
                {
                    case "ChangeTime":
                    case "CreateTime":
                    case "DocumentDate":
                    case "EnableDate":
                    case "OperateTime":
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "OriginValue":
                    case "Depreciation":
                        right = Expression.Constant(decimal.Parse(operand));
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
            var e = db.ArEquipLogs;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(ArEquipLogModels) }, Expression.Constant(e), pred);

            IQueryable<ArEquipLogModels> list = db.ArEquipLogs.AsQueryable().Provider.CreateQuery<ArEquipLogModels>(expr);

            return list;
        }

        private List<ArEquipLogModels> getSelected(IQueryable<ArEquipLogModels> l)
        {
            List<ArEquipLogModels> list = new List<ArEquipLogModels>();
            List<ArEquipLogModels> list_origin = l.ToList();
            foreach (ArEquipLogModels e in list_origin)
            {
                if (Request.Form["Checked" + e.EquipmentID + e.OperateTime.ToBinary()] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /ArEquipLog/Details/5
        public async Task<ActionResult> Details(int id)
        {
            ArEquipLogModels arequiplogmodels = await db.ArEquipLogs.FindAsync(id);
            if (arequiplogmodels == null)
            {
                return HttpNotFound();
            }

            EquipLogModels e = await db.EquipLogs.FindAsync(arequiplogmodels.EquipmentID);
            ViewData["origin"] = e;
            return View(arequiplogmodels);
        }

        // GET: /ArEquipLog/Rollback/5
        public async Task<ActionResult> Rollback(int id)
        {
            ArEquipLogModels a = await db.ArEquipLogs.FindAsync(id);
            if (a == null)
            {
                return HttpNotFound();
            }
            EquipLogModels origin = await db.EquipLogs.FindAsync(a.EquipmentID);

            ArEquipmentModels.OperatorType change;

            switch (a.Operator)
            {
                case ArEquipmentModels.OperatorType.创建:
                    if (origin == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "表中已不存在此记录");
                    }
                    db.EquipLogs.Remove(origin);
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
                        db.EquipLogs.Add(origin);
                    }
                    origin = new EquipLogModels();
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
                ArEquipLogModels ar = new ArEquipLogModels(origin);
                ar.Operator = change;
                db.ArEquipLogs.Add(ar);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GET: /ArEquipLog/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArEquipLogModels arequiplogmodels = await db.ArEquipLogs.FindAsync(id);
            if (arequiplogmodels == null)
            {
                return HttpNotFound();
            }
            return View(arequiplogmodels);
        }

        // POST: /ArEquipLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ArEquipLogModels arequiplogmodels = await db.ArEquipLogs.FindAsync(id);
            db.ArEquipLogs.Remove(arequiplogmodels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /ArEquipLog/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<ArEquipLogModels> l = getQuery();
            List<ArEquipLogModels> list = getSelected(l);
            foreach (ArEquipLogModels e in list)
            {
                db.ArEquipLogs.Remove(e);
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

        // GET: /ArEquipLog/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<ArEquipLogModels> list = db.ArEquipLogs.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "记录编号",
                "设备编号",
        "使用部门",
        "设备名称",
        "型号",
        "规格",
        "立卡时间",
        "启用时间",
        "原值",
        "累计折旧",
        "一级地点",
        "二级地点",
        "三级地点",
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Department);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipDes);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Type);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Spec);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.DocumentDate);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EnableDate);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OriginValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Depreciation);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Spot1);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Spot2);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Spot3);
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
            return File(fileStream, "application/ms-excel", "设备台账 - 历史记录.xls");
        }
    }
}
