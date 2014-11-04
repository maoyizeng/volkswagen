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

namespace Volkswagen.Controllers
{
    public class FileController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /File/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<FileModels> list = getQuery(false);
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

            return View(list.ToList().AsPagination(page ?? 1, 100));
        }

        private IQueryable<FileModels> getQuery(bool post = true)
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(FileModels), "p");
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
                Expression left = Expression.Property(param, typeof(FileModels).GetProperty(field));
                //[operandn]
                Expression right = Expression.Constant(operand);
                Expression result;

                switch (field)
                {
                    case "ChangeTime":
                    case "CreateTime":
                        right = Expression.Constant(Convert.ToDateTime(operand));
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
            var e = db.Files;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(FileModels) }, Expression.Constant(e), pred);

            IQueryable<FileModels> list = db.Files.AsQueryable().Provider.CreateQuery<FileModels>(expr);

            return list;
        }

        private List<FileModels> getSelected(IQueryable<FileModels> l)
        {
            List<FileModels> list = new List<FileModels>();
            List<FileModels> list_origin = l.ToList();
            foreach (FileModels e in list_origin)
            {
                if (Request.Form["Checked" + e.FileName] != "false")
                {
                    list.Add(e);
                }
            }

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

        // POST: /File/EditMultiple/
        //[HttpPost]
        public async Task<ActionResult> EditMultiple()
        {
            IQueryable<FileModels> l = getQuery();
            List<FileModels> list = getSelected(l);
            if (ViewData["list"] == null) ViewData["list"] = list;
            //string key = list.First().FileID;
            //return RedirectToAction("Edit", new { id = key });
            return RedirectToAction("ChangeMultiple", new { Filemodels = new FileModels() });
        }

        // POST: /File/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "FileName,Class,EquipmentID,EquipDes,Charger,File,ChangeTime,Changer,CreateTime,Creator")] FileModels filemodels)
        {
            bool changed = false;
            List<FileModels> l = new List<FileModels>();
            for (int i = 0; ; i++)
            {
                string id = Request.Form["item" + i];
                if (Request.Form["item" + i] == null) break;
                FileModels e = db.Files.Find(id);
                l.Add(e);
                ArFileModels ar = new ArFileModels(e);
                if (filemodels.EquipmentID != null && ModelState.IsValidField("EquipmentID")) e.EquipmentID = filemodels.EquipmentID;
                if (filemodels.EquipDes != null && ModelState.IsValidField("EquipDes")) e.EquipDes = filemodels.EquipDes;
                if (filemodels.Class != null && ModelState.IsValidField("Class")) e.Class = filemodels.Class;
                if (filemodels.Charger != null && ModelState.IsValidField("Charger")) e.Charger = filemodels.Charger;

                if (db.Entry(e).State == EntityState.Modified)
                {
                    e.Changer = User.Identity.Name;
                    e.ChangeTime = DateTime.Now;
                    int x = await db.SaveChangesAsync();
                    if (x != 0)
                    {
                        changed = true;
                        ar.Operator = "Update";
                        db.ArFiles.Add(ar);
                        await db.SaveChangesAsync();
                    }
                }
            }
            if (changed)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewData["list"] = l;
                return View(new FileModels());
            }
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
            FileModels toDelete = await db.Files.FindAsync(id);
            db.Files.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ArFileModels ar = new ArFileModels(toDelete);
                ar.Operator = "Delete";
                db.ArFiles.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /File/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<FileModels> l = getQuery();
            List<FileModels> list = getSelected(l);
            foreach (FileModels e in list)
            {
                db.Files.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ArFileModels ar = new ArFileModels(e);
                    ar.Operator = "Delete";
                    db.ArFiles.Add(ar);
                    await db.SaveChangesAsync();
                }
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

        // POST: /File/FileUpload
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult FileUpload(HttpPostedFileBase[] files)
        {
            string key = Request.Form["key"];
            string fullname = "";

            foreach (HttpPostedFileBase file in files)
            {
                if (file != null)
                {
                    string filePath = Path.Combine((AppDomain.CurrentDomain.BaseDirectory + @"file\"), Path.GetFileName(file.FileName));
                    file.SaveAs(filePath);
                    fullname += "$" + file.FileName;
                }
            }

            FileModels e = db.Files.Find(key);
            // TODO - check e;
            e.File = fullname;
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = key });

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
