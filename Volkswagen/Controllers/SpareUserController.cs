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
using MvcContrib.UI.Grid;
using System.Linq.Expressions;
using System.IO;
using System.Linq.Dynamic;
using MvcContrib.Sorting;
using System.Text;

namespace Volkswagen.Controllers
{
    public class SpareUserController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /SpareUser/
        /*public async Task<ActionResult> Index(GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<SpareUserModels> list = db.SpareUsers.Where("1 = 1");
            if (!string.IsNullOrEmpty(model.Column))
            {
                list = list.OrderBy(model.Column);
            }
            else
            {
                return View(await db.SpareUsers.ToListAsync());
            }
            return View(list);
        }*/

        // GET: /SpareUser/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareUserModels spareusermodels = await db.SpareUsers.FindAsync(id);
            if (spareusermodels == null)
            {
                return HttpNotFound();
            }
            return View(spareusermodels);
        }

        // GET: /SpareUser/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            IQueryable<SpareUserModels> list = db.SpareUsers.Where("1 = 1");
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
                return View(await db.SpareUsers.ToListAsync());
            }
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> Index()
        {
            ParameterExpression param = Expression.Parameter(typeof(SpareUserModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++)
            {
                string field = Request.Form["field" + n];
                string op = Request.Form["op" + n];
                string operand = Request.Form["operand" + n];
                if (string.IsNullOrEmpty(field)) break;

                Expression left = Expression.Property(param, typeof(SpareUserModels).GetProperty(field));
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
                    default:
                        result = Expression.Equal(left, right);
                        break;
                }
                filter = Expression.And(filter, result);
            }

            Expression pred = Expression.Lambda(filter, param);

            var e = db.SpareUsers;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(SpareUserModels) }, Expression.Constant(e), pred);

            IQueryable<SpareUserModels> list = db.SpareUsers.AsQueryable().Provider.CreateQuery<SpareUserModels>(expr);


            return View(list);
        }

        // GET: /SpareUser/Create
        public ActionResult Create()
        {
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID");
            ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes");
            return View();
        }

        // POST: /SpareUser/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="UserID,SpareID,SpareDes,Type,InValue,OutValue,User,UseTime,ActualUse,ChangeTime,Changer,CreateTime,Creator")] SpareUserModels spareusermodels)
        {
            if (ModelState.IsValid)
            {
                spareusermodels.Changer = User.Identity.Name;
                spareusermodels.Creator = User.Identity.Name;
                spareusermodels.CreateTime = DateTime.Now;
                spareusermodels.ChangeTime = DateTime.Now;
                db.SpareUsers.Add(spareusermodels);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ArSpareUserModels ar = new ArSpareUserModels(spareusermodels);
                    ar.Operator = "Create";
                    db.ArSpareUsers.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }

            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID", spareusermodels.SpareID);
            ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes", spareusermodels.SpareDes);
            return View(spareusermodels);
        }

        // GET: /SpareUser/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareUserModels spareusermodels = await db.SpareUsers.FindAsync(id);
            if (spareusermodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID", spareusermodels.SpareID);
            ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes", spareusermodels.SpareDes);
            return View(spareusermodels);
        }

        // POST: /SpareUser/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="UserID,SpareID,SpareDes,Type,InValue,OutValue,User,UseTime,ActualUse,ChangeTime,Changer,CreateTime,Creator")] SpareUserModels spareusermodels)
        {
            if (ModelState.IsValid)
            {
                var toUpdate = db.SpareUsers.Find(spareusermodels.UserID);

                spareusermodels.Changer = User.Identity.Name;
                spareusermodels.ChangeTime = DateTime.Now;
                spareusermodels.Creator = toUpdate.Creator;
                spareusermodels.CreateTime = toUpdate.CreateTime;

                db.Entry(toUpdate).State = EntityState.Detached;
                db.Entry(spareusermodels).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArSpareUserModels ar = new ArSpareUserModels(toUpdate);
                    ar.Operator = "Update";
                    db.ArSpareUsers.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID", spareusermodels.SpareID);
            ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes", spareusermodels.SpareDes);
            return View(spareusermodels);
        }

        // GET: /SpareUser/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareUserModels spareusermodels = await db.SpareUsers.FindAsync(id);
            if (spareusermodels == null)
            {
                return HttpNotFound();
            }
            return View(spareusermodels);
        }

        // POST: /SpareUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SpareUserModels toDelete = await db.SpareUsers.FindAsync(id);
            db.SpareUsers.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ArSpareUserModels ar = new ArSpareUserModels(toDelete);
                ar.Operator = "Delete";
                db.ArSpareUsers.Add(ar);
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

        // GET: /SpareUser/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<SpareUserModels> list = db.SpareUsers.ToList();

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
        "创建人"};
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
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "备件领用记录.xls");
        }
    }
}
