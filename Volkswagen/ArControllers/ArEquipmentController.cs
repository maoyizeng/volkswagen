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
    public class ArEquipmentController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArEquipment/
        public async Task<ActionResult> Index(int? page, GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<ArEquipmentModels> list = db.ArEquipments.Where("1 = 1");
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
                return View(db.ArEquipments.ToList().AsPagination(page ?? 1, 200));
            }
            //list = list.AsPagination(page ?? 1, 5);
            return View(list.ToList().AsPagination(page ?? 1, 200));
        }

        [HttpPost]
        public async Task<ActionResult> Index(int? page)
        {
            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;

            IQueryable<ArEquipmentModels> list = getQuery();

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

        private IQueryable<ArEquipmentModels> getQuery()
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ArEquipmentModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArEquipmentModels).GetProperty(field));
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
            var e = db.ArEquipments;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(ArEquipmentModels) }, Expression.Constant(e), pred);

            IQueryable<ArEquipmentModels> list = db.ArEquipments.AsQueryable().Provider.CreateQuery<ArEquipmentModels>(expr);

            return list;
        }

        private List<ArEquipmentModels> getSelected(IQueryable<ArEquipmentModels> l)
        {
            List<ArEquipmentModels> list = new List<ArEquipmentModels>();
            List<ArEquipmentModels> list_origin = l.ToList();
            foreach (ArEquipmentModels e in list_origin)
            {
                if (Request.Form["Checked" + e.EquipmentID + e.OperateTime.ToBinary()] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /ArEquipment/Details/5
        public async Task<ActionResult> Details(string id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //ArEquipmentModels arequipmentmodels = await db.ArEquipments.Where(p => p.EquipmentID == id && p.OperateTime.Equals(new DateTime(opt))).FirstAsync();
            ArEquipmentModels arequipmentmodels = await db.ArEquipments.FindAsync(id, op, new DateTime(opt));
            
            if (arequipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(arequipmentmodels);
        }

        // GET: /ArEquipment/Rollback/5
        public async Task<ActionResult> Rollback(string id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArEquipmentModels a = await db.ArEquipments.FindAsync(id, op, new DateTime(opt));
            if (a == null)
            {
                return HttpNotFound();
            }
            EquipmentModels origin = await db.Equipments.FindAsync(id);
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
                origin = new EquipmentModels();
                origin.upcast(a);
                origin.Changer = User.Identity.Name;
                origin.Creator = User.Identity.Name;
                origin.CreateTime = DateTime.Now;
                origin.ChangeTime = DateTime.Now;
                change = "Create";
                db.Equipments.Add(origin);
            }

            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ArEquipmentModels ar = new ArEquipmentModels(origin);
                ar.Operator = change;
                db.ArEquipments.Add(ar);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GET: /ArEquipment/Delete/5
        public async Task<ActionResult> Delete(string id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArEquipmentModels arequipmentmodels = await db.ArEquipments.FindAsync(id, op, new DateTime(opt));
            if (arequipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(arequipmentmodels);
        }

        // POST: /ArEquipment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id, string op, long opt)
        {
            ArEquipmentModels arequipmentmodels = await db.ArEquipments.FindAsync(id, op, new DateTime(opt));
            db.ArEquipments.Remove(arequipmentmodels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /ArEquipment/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<ArEquipmentModels> l = getQuery();
            List<ArEquipmentModels> list = getSelected(l);
            foreach (ArEquipmentModels e in list)
            {
                db.ArEquipments.Remove(e);
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

        // GET: /ArEquipment/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<ArEquipmentModels> list = db.ArEquipments.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "设备编号",
                "设备名称",
                "负责人",
                "所在工段",
                "车间生产线",
                "点检",
                "日常保养",
                "巡检",
                "需更新否",
                "最后修改时间",
                "修改人",
                "创建时间",
                "创建人",
                "备注",
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
                sbHtml.AppendFormat(format, i.EquipmentID);
                sbHtml.AppendFormat(format, i.EquipDes);
                sbHtml.AppendFormat(format, i.Person);
                sbHtml.AppendFormat(format, i.Section);
                sbHtml.AppendFormat(format, i.WSArea);
                sbHtml.AppendFormat(format, i.ItemInspect);
                sbHtml.AppendFormat(format, i.RegularCare);
                sbHtml.AppendFormat(format, i.Check);
                sbHtml.AppendFormat(format, i.RoutingInspect);
                // TODO - photo?
                sbHtml.AppendFormat(format, i.ChangeTime);
                sbHtml.AppendFormat(format, i.Changer);
                sbHtml.AppendFormat(format, i.CreateTime);
                sbHtml.AppendFormat(format, i.Creator);
                sbHtml.AppendFormat(format, i.Remark);
                sbHtml.AppendFormat(format, i.Operator);
                sbHtml.AppendFormat(format, i.OperateTime);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "历史记录 - 设备履历.xls");
        }
    }
}
