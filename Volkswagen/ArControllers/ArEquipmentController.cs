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
    [Authorize(Roles="Admin")]
    public class ArEquipmentController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArEquipment/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            //PrepareSelectItems();
            //return View(await db.Equipments.ToListAsync());

            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<ArEquipmentModels> list = getQuery(false);
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
            //list = list.AsPagination(page ?? 1, 5);
            //IPagination<Volkswagen.Models.EquipmentModels> l = list.ToList().AsPagination(page ?? 1, 200);
            return View(list.ToList().AsPagination(page ?? 1, 100));
        }

        [HttpPost]
        public async Task<ActionResult> Index(int? page, string selected_item)
        {
            //IQueryable<EquipmentModels> list = ViewData.Model as IQueryable<EquipmentModels>;
            //IQueryable<EquipmentModels> list = db.Equipments.Where("1 = 1");

            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

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

            //IPagination<Volkswagen.Models.EquipmentModels> l = list.ToList().AsPagination(page ?? 1, 5);
            return View(list.ToList().AsPagination(page ?? 1, 100));
            //return View(l);
        }

        private IQueryable<ArEquipmentModels> getQuery(bool post = true)
        {
            /*string query = "1 = 1";

            ParameterExpression param = Expression.Parameter(typeof(EquipmentModels), "p");

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

                if (Expression.Property(param, typeof(EquipmentModels).GetProperty(field)).Type == typeof(string) && (!op.Equals("6")))
                {
                    operand = "\"" + operand + "\"";
                }
                else if (Expression.Property(param, typeof(EquipmentModels).GetProperty(field)).Type.MemberType.GetType().IsEnum)
                {
                    Type t = Expression.Property(param, typeof(EquipmentModels).GetProperty(field)).Type.GenericTypeArguments[0];
                    operand = Convert.ToInt32(Enum.Parse(t, operand)) + "";
                }

                switch (op)
                {
                    case "0":
                        query += " AND " + field + " = " + operand;
                        break;
                    case "1":
                        query += " AND " + field + " > " + operand;
                        break;
                    case "2":
                        query += " AND " + field + " < " + operand;
                        break;
                    case "3":
                        query += " AND " + field + " >= " + operand;
                        break;
                    case "4":
                        query += " AND " + field + " <= " + operand;
                        break;
                    case "5":
                        query += " AND " + field + " <> " + operand;
                        break;
                    case "6": //Contain
                        query += " AND " + field + " like %" + operand + "%";
                        break;
                    default:
                        query += " AND " + field + " = " + operand;
                        break;
                }
            }

            IQueryable<EquipmentModels> list = db.Equipments.Where(query);
            return list;
             * */

            //p
            ParameterExpression param = Expression.Parameter(typeof(ArEquipmentModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArEquipmentModels).GetProperty(field));
                //[operandn]
                Expression right = Expression.Constant(operand);

                switch (field)
                {
                    case "RecordID":
                        right = Expression.Constant(int.Parse(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "WSArea":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(EquipmentModels.WSNames), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "ItemInspect":
                    case "RegularCare":
                    case "Check":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(EquipmentModels.ThereBe), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "RoutingInspect":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(EquipmentModels.YesNo), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "Operator":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(ArEquipmentModels.OperatorType), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "ChangeTime":
                    case "CreateTime":
                    case "OperateTime":
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    default:
                        break;
                }

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
        public async Task<ActionResult> Details(int id)
        {
            //ArEquipmentModels arequipmentmodels = await db.ArEquipments.Where(p => p.EquipmentID == id && p.OperateTime.Equals(new DateTime(opt))).FirstAsync();
            ArEquipmentModels arequipmentmodels = await db.ArEquipments.FindAsync(id);
            
            if (arequipmentmodels == null)
            {
                return HttpNotFound();
            }

            
            EquipmentModels e = await db.Equipments.FindAsync(arequipmentmodels.EquipmentID);
            ViewData["origin"] = e;

            return View(arequipmentmodels);
        }

        // GET: /ArEquipment/Rollback/5
        // 回滚机制
        // 回滚的机制为, 回滚到这一条的状态, 也就是说, 将这一条记录到目前状态之间所有的修改全部无视而直接覆盖
        public async Task<ActionResult> Rollback(int id)
        {
            ArEquipmentModels a = await db.ArEquipments.FindAsync(id);
            if (a == null)
            {
                return HttpNotFound();
            }
            EquipmentModels origin = await db.Equipments.FindAsync(a.EquipmentID);

            ArEquipmentModels.OperatorType change;

            switch (a.Operator)
            {
                case ArEquipmentModels.OperatorType.创建:
                    // 如果是创建操作 但是目前没有这条记录 说明之后已经被删 报错
                    if (origin == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "表中已不存在此记录");
                    }
                    // 存在 那么删除掉这条被创建的记录
                    db.Equipments.Remove(origin);
                    change = ArEquipmentModels.OperatorType.删除;
                    break;
                case ArEquipmentModels.OperatorType.修改:
                    // 如果是修改操作 但是目前没有这条记录 说明之后已经被删 报错
                    if (origin == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "表中已不存在此记录");
                    }
                    // 直接覆盖当前的记录
                    origin.upcast(a);
                    change = ArEquipmentModels.OperatorType.修改;
                    break;
                case ArEquipmentModels.OperatorType.删除:
                    // 如果是删除 但是目前还是有这条记录的 说明之后又被创建了 因此这次操作是一次修改操作 并强制覆盖
                    if (origin != null)
                    {
                        change = ArEquipmentModels.OperatorType.修改;
                    }
                    else
                    {
                    // 不存在这条记录了 那么再重新创建这条被删的记录
                        change = ArEquipmentModels.OperatorType.创建;
                        db.Equipments.Add(origin);
                    }
                    origin = new EquipmentModels();
                    origin.upcast(a);
                    origin.Creator = User.Identity.Name;
                    origin.CreateTime = DateTime.Now;                    
                    break;
                default:
                    // 缺省 不应该跑进来的 随便操作一下
                    if (origin == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    change = ArEquipmentModels.OperatorType.修改;
                    break;
            }

            // 必要记录修改
            origin.Changer = User.Identity.Name;
            origin.ChangeTime = DateTime.Now;

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
        public async Task<ActionResult> Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArEquipmentModels arequipmentmodels = await db.ArEquipments.FindAsync(id);
            if (arequipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(arequipmentmodels);
        }

        // POST: /ArEquipment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ArEquipmentModels arequipmentmodels = await db.ArEquipments.FindAsync(id);
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
                "记录编号",
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
                sbHtml.AppendFormat(format, i.RecordID);
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
