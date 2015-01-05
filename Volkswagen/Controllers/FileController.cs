using ICSharpCode.SharpZipLib.Zip;
using MvcContrib.Pagination;
using MvcContrib.Sorting;
using MvcContrib.UI.Grid;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Volkswagen.DAL;
using Volkswagen.Models;

namespace Volkswagen.Controllers
{
    [UserAuthorized]
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
                var ss = Request.Form["Checked" + e.FileName];
                if ((!string.IsNullOrEmpty(ss)) && (ss != "false"))
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
            if (db.Equipments.Find(filemodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(filemodels);
            }
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
                    ar.Operator = ArEquipmentModels.OperatorType.创建;
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
            if (db.Equipments.Find(filemodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(filemodels);
            }
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
                    ar.Operator = ArEquipmentModels.OperatorType.修改;
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
            return View("ChangeMultiple", new FileModels());
        }

        // POST: /File/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "FileName,Class,EquipmentID,EquipDes,Charger,File,ChangeTime,Changer,CreateTime,Creator")] FileModels filemodels)
        {
            if ((!string.IsNullOrEmpty(filemodels.EquipmentID)) && (db.Equipments.Find(filemodels.EquipmentID) == null))
            {
                ViewData["valid"] = "no_foreign";
                return View(filemodels);
            }

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
                        ar.Operator = ArEquipmentModels.OperatorType.修改;
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
            ArFileModels ar = new ArFileModels(toDelete);
            db.Files.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.删除;
                db.ArFiles.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /File/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple(int? page, string selected_item)
        {
            IQueryable<FileModels> l = getQuery();
            List<FileModels> list = getSelected(l);
            foreach (FileModels e in list)
            {
                ArFileModels ar = new ArFileModels(e);
                db.Files.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ar.Operator = ArEquipmentModels.OperatorType.删除;
                    db.ArFiles.Add(ar);
                    await db.SaveChangesAsync();
                }
            }
            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;
            l = getQuery();

            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    l = l.OrderBy(model.Column + " desc");
                }
                else
                {
                    l = l.OrderBy(model.Column + " asc");
                }
            }
            return View("Index", l.ToList().AsPagination(page ?? 1, 100));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // POST: /File/DownloadMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadMultiple(int? page, string selected_item)
        {
            IQueryable<FileModels> l = getQuery();
            List<FileModels> list = getSelected(l);

            Random random = new Random();
            int ranfolder = random.Next();
            string folder = AppDomain.CurrentDomain.BaseDirectory + @"files\tmp\" + ranfolder;
            Directory.CreateDirectory(folder);

            foreach (FileModels e in list)
            {
                string[] sArray = Regex.Split(e.File, "[$]");

                for (int i = 1; i < sArray.Length; i++)
                {
                    System.IO.File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"files\" + e.Class + @"\" + e.EquipDes + @"\" + sArray[i], folder + @"\" + sArray[i], true);
                }
            }

            // 压缩
            FastZip fz = new FastZip();
            fz.CreateZip(folder + @"\..\" + ranfolder + ".zip", folder, false, "");
            fz = null;

            // 返回
            return File(folder + @"\..\" + ranfolder + ".zip", "application/zip", "多个文件.zip");
        }

        // POST: /Equipment/FileRemove
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult FileRemove()
        {
            string key = Request.Form["key2"];

            FileModels e = db.Files.Find(key);
            if (e == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string origin_photo = e.File;
            string new_photo = "";
            string[] sArray = Regex.Split(origin_photo, "[$]");

            for (int i = 1; i < sArray.Length; i++)
            {
                if (Request.Form["photo_" + i] != "on")
                {
                    new_photo += "$" + sArray[i];
                }
            }

            ArFileModels ar = new ArFileModels(e);
            // TODO - check e;
            e.Changer = User.Identity.Name;
            e.ChangeTime = DateTime.Now;
            e.File = new_photo;
            int x = db.SaveChanges();

            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.修改;
                db.ArFiles.Add(ar);
                db.SaveChanges();
            }
            return RedirectToAction("Edit", new { id = key });
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
            FileModels e = db.Files.Find(key);
            if (e == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string directory = AppDomain.CurrentDomain.BaseDirectory + @"files\" + e.Class + @"\";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            directory += e.EquipDes + @"\";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            foreach (HttpPostedFileBase file in files)
            {
                if (file != null)
                {
                    string filename = DateTime.Now.ToString("yy-MM-dd HH-mm-ss") + " - " + Path.GetFileName(file.FileName);
                    // 构造需要保存的路径并保存, 然后将$...加到字符串中
                    string filePath = Path.Combine(directory, filename);
                    file.SaveAs(filePath);
                    fullname += "$" + filename;
                }
            }
            
            ArFileModels ar = new ArFileModels(e);
            e.Changer = User.Identity.Name;
            e.ChangeTime = DateTime.Now;
            e.File += fullname;
            int x = db.SaveChanges();

            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.修改;
                db.ArFiles.Add(ar);
                db.SaveChanges();
            }
            return RedirectToAction("Edit", new { id = key });

        }

        // GET: /File/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();

            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;

            var l = getQuery();

            // 排序
            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    l = l.OrderBy(model.Column + " desc");
                }
                else
                {
                    l = l.OrderBy(model.Column + " asc");
                }
            }

            var list = l.ToList();

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
