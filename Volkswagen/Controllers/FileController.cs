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

namespace Volkswagen.Controllers
{
    public class FileController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /File/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            //PrepareSelectItems();
            //return View(await db.Files.ToListAsync());

            ViewData["model"] = model;

            IQueryable<FileModels> list = db.Files.Where("1 = 1");
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
                return View(await db.Files.ToListAsync());
            }
            //list = list.AsPagination(page ?? 1, 5);
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> Index()
        {
            //IQueryable<FileModels> list = ViewData.Model as IQueryable<FileModels>;
            //IQueryable<FileModels> list = db.Files.Where("1 = 1");

            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;

            IQueryable<FileModels> list = getQuery();

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

        private IQueryable<FileModels> getQuery()
        {
            ParameterExpression param = Expression.Parameter(typeof(FileModels), "p");
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

                Expression left = Expression.Property(param, typeof(FileModels).GetProperty(field));
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

            var e = db.Files;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(FileModels) }, Expression.Constant(e), pred);

            IQueryable<FileModels> list = db.Files.AsQueryable().Provider.CreateQuery<FileModels>(expr);

            return list;
        }

        // GET: /File/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileModels filemodels = await db.Files.FindAsync(id);
            if (filemodels == null)
            {
                return HttpNotFound();
            }
            return View(filemodels);
        }

        // GET: /File/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /File/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="FileName,Class,EquipmentID,EquipDes,Charger,File,ChangeTime,Changer,CreateTime,Creator")] FileModels filemodels)
        {
            if (ModelState.IsValid)
            {
                filemodels.Changer = User.Identity.Name;
                filemodels.Creator = User.Identity.Name;
                filemodels.CreateTime = DateTime.Now;
                filemodels.ChangeTime = DateTime.Now;
                db.Files.Add(filemodels);
                 
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ArFileModels ar = new ArFileModels(filemodels);
                    ar.Operator = "Create";
                    db.ArFiles.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }

            return View(filemodels);
        }

        // GET: /File/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileModels filemodels = await db.Files.FindAsync(id);
            if (filemodels == null)
            {
                return HttpNotFound();
            }
            return View(filemodels);
        }

        // POST: /File/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="FileName,Class,EquipmentID,EquipDes,Charger,File,ChangeTime,Changer,CreateTime,Creator")] FileModels filemodels)
        {
            if (ModelState.IsValid)
            {
                var toUpdate = db.Files.Find(filemodels.FileName);

                filemodels.Changer = User.Identity.Name;
                filemodels.ChangeTime = DateTime.Now;
                filemodels.Creator = toUpdate.Creator;
                filemodels.CreateTime = toUpdate.CreateTime;


                db.Entry(toUpdate).State = EntityState.Detached;
                db.Entry(filemodels).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArFileModels ar = new ArFileModels(toUpdate);
                    ar.Operator = "Update";
                    db.ArFiles.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            return View(filemodels);
        }

        // GET: /File/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileModels filemodels = await db.Files.FindAsync(id);
            if (filemodels == null)
            {
                return HttpNotFound();
            }
            return View(filemodels);
        }

        // POST: /File/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            FileModels filemodels = await db.Files.FindAsync(id);
            db.Files.Remove(filemodels);
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

        // GET: /File/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<FileModels> list = db.Files.ToList();

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
                "创建人" };
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
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "文件库.xls");
        }
    }
}
