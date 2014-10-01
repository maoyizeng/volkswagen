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
    public class ArUserController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArUser/
        public async Task<ActionResult> Index(int? page, GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<ArUserModels> list = db.ArUsers.Where("1 = 1");
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
                return View(db.ArUsers.ToList().AsPagination(page ?? 1, 200));
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

            IQueryable<ArUserModels> list = getQuery();

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

        private IQueryable<ArUserModels> getQuery()
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(ArUserModels), "p");
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
                Expression left = Expression.Property(param, typeof(ArUserModels).GetProperty(field));
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
            var e = db.ArUsers;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(ArUserModels) }, Expression.Constant(e), pred);

            IQueryable<ArUserModels> list = db.ArUsers.AsQueryable().Provider.CreateQuery<ArUserModels>(expr);

            return list;
        }

        private List<ArUserModels> getSelected(IQueryable<ArUserModels> l)
        {
            List<ArUserModels> list = new List<ArUserModels>();
            List<ArUserModels> list_origin = l.ToList();
            foreach (ArUserModels e in list_origin)
            {
                if (Request.Form["Checked" + e.UserID + e.OperateTime.ToBinary()] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /ArUser/Details/5
        public async Task<ActionResult> Details(int? id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArUserModels arusermodels = await db.ArUsers.FindAsync(id, op, new DateTime(opt));
            if (arusermodels == null)
            {
                return HttpNotFound();
            }
            return View(arusermodels);
        }

        // GET: /ArUser/Rollback/5
        public async Task<ActionResult> Rollback(int? id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArUserModels a = await db.ArUsers.FindAsync(id, op, new DateTime(opt));
            if (a == null)
            {
                return HttpNotFound();
            }
            UserModels origin = await db.Users.FindAsync(id);
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
                origin = new UserModels();
                origin.upcast(a);
                origin.Changer = User.Identity.Name;
                origin.Creator = User.Identity.Name;
                origin.CreateTime = DateTime.Now;
                origin.ChangeTime = DateTime.Now;
                change = "Create";
                db.Users.Add(origin);
            }

            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ArUserModels ar = new ArUserModels(origin);
                ar.Operator = change;
                db.ArUsers.Add(ar);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GET: /ArUser/Delete/5
        public async Task<ActionResult> Delete(int? id, string op, long opt)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArUserModels arusermodels = await db.ArUsers.FindAsync(id, op, new DateTime(opt));
            if (arusermodels == null)
            {
                return HttpNotFound();
            }
            return View(arusermodels);
        }

        // POST: /ArUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id, string op, long opt)
        {
            ArUserModels arusermodels = await db.ArUsers.FindAsync(id, op, new DateTime(opt));
            db.ArUsers.Remove(arusermodels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /ArUser/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<ArUserModels> l = getQuery();
            List<ArUserModels> list = getSelected(l);
            foreach (ArUserModels e in list)
            {
                db.ArUsers.Remove(e);
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

        // GET: /ArUser/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<ArUserModels> list = db.ArUsers.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "简称",
                "姓名",
                "工号",
                "电话",
                "手机",
                "生日",
                "进入公司时间",
                "职务",
                "政治面貌",
                "住址",
                "技能特长",
                "工作经验",
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

            string format = "<td style='font-size: 12px;height:20px;'>{0}</td>";
            foreach (var i in list)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat(format, i.Breviary);
                sbHtml.AppendFormat(format, i.Name);
                sbHtml.AppendFormat(format, i.Number);
                sbHtml.AppendFormat(format, i.Telephone);
                sbHtml.AppendFormat(format, i.Mobile);
                sbHtml.AppendFormat(format, i.Birthday);
                sbHtml.AppendFormat(format, i.EntryDate);
                sbHtml.AppendFormat(format, i.Position);
                sbHtml.AppendFormat(format, i.PoliticalStatus);
                sbHtml.AppendFormat(format, i.Address);
                sbHtml.AppendFormat(format, i.Skill);
                sbHtml.AppendFormat(format, i.Experience);
                sbHtml.AppendFormat(format, i.Remark);
                // TODO - image?
                sbHtml.AppendFormat(format, i.UserID);
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
            return File(fileStream, "application/ms-excel", "人员信息 - 历史记录.xls");
        }
    }
}
