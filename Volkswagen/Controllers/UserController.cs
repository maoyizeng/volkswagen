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
    public class UserController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /User/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<UserModels> list = db.Users.Where("1 = 1");
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
                return View(await db.Users.ToListAsync());
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

            IQueryable<UserModels> list = getQuery();

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

        private IQueryable<UserModels> getQuery()
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(UserModels), "p");
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
                Expression left = Expression.Property(param, typeof(UserModels).GetProperty(field));
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
            var e = db.Users;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(UserModels) }, Expression.Constant(e), pred);

            IQueryable<UserModels> list = db.Users.AsQueryable().Provider.CreateQuery<UserModels>(expr);

            return list;
        }

        private List<UserModels> getSelected(IQueryable<UserModels> l)
        {
            List<UserModels> list = new List<UserModels>();
            List<UserModels> list_origin = l.ToList();
            foreach (UserModels e in list_origin)
            {
                if (Request.Form["Checked" + e.UserID] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /User/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserModels usermodels = await db.Users.FindAsync(id);
            if (usermodels == null)
            {
                return HttpNotFound();
            }
            return View(usermodels);
        }

        // GET: /User/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /User/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="UserID,Breviary,Name,Number,Telephone,Mobile,Birthday,EntryDate,Position,PoliticalStatus,Address,Skill,Experience,Remark,Image,ChangeTime,Changer,CreateTime,Creator")] UserModels usermodels)
        {
            if (ModelState.IsValid)
            {
                usermodels.Changer = User.Identity.Name;
                usermodels.Creator = User.Identity.Name;
                usermodels.CreateTime = DateTime.Now;
                usermodels.ChangeTime = DateTime.Now;
                db.Users.Add(usermodels);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ArUserModels ar = new ArUserModels(usermodels);
                    ar.Operator = "Create";
                    db.ArUsers.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }

            return View(usermodels);
        }

        // GET: /User/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserModels usermodels = await db.Users.FindAsync(id);
            if (usermodels == null)
            {
                return HttpNotFound();
            }
            return View(usermodels);
        }

        // POST: /User/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="UserID,Breviary,Name,Number,Telephone,Mobile,Birthday,EntryDate,Position,PoliticalStatus,Address,Skill,Experience,Remark,Image,ChangeTime,Changer,CreateTime,Creator")] UserModels usermodels)
        {
            if (ModelState.IsValid)
            {
                var toUpdate = db.Users.Find(usermodels.UserID);

                usermodels.Changer = User.Identity.Name;
                usermodels.ChangeTime = DateTime.Now;
                usermodels.Creator = toUpdate.Creator;
                usermodels.CreateTime = toUpdate.CreateTime;

                db.Entry(toUpdate).State = EntityState.Detached;
                db.Entry(usermodels).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArUserModels ar = new ArUserModels(toUpdate);
                    ar.Operator = "Update";
                    db.ArUsers.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            return View(usermodels);
        }

        // POST: /User/EditMultiple/
        //[HttpPost]
        public async Task<ActionResult> EditMultiple()
        {
            IQueryable<UserModels> l = getQuery();
            List<UserModels> list = getSelected(l);
            if (ViewData["list"] == null) ViewData["list"] = list;
            //string key = list.First().UserID;
            //return RedirectToAction("Edit", new { id = key });
            return RedirectToAction("ChangeMultiple", new { Usermodels = new UserModels() });
        }

        // POST: /User/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "UserID,Breviary,Name,Number,Telephone,Mobile,Birthday,EntryDate,Position,PoliticalStatus,Address,Skill,Experience,Remark,Image,ChangeTime,Changer,CreateTime,Creator")] UserModels usermodels)
        {
            bool changed = false;
            List<UserModels> l = new List<UserModels>();
            for (int i = 0; ; i++)
            {
                string id = Request.Form["item" + i];
                if (Request.Form["item" + i] == null) break;
                UserModels e = db.Users.Find(id);
                l.Add(e);
                ArUserModels ar = new ArUserModels(e);
                if (usermodels.Breviary != null && ModelState.IsValidField("Breviary")) e.Breviary = usermodels.Breviary;
                if (usermodels.Name != null && ModelState.IsValidField("Name")) e.Name = usermodels.Name;
                if (usermodels.Number != null && ModelState.IsValidField("Number")) e.Number = usermodels.Number;
                if (usermodels.Telephone != null && ModelState.IsValidField("Telephone")) e.Telephone = usermodels.Telephone;
                if (usermodels.Mobile != null && ModelState.IsValidField("Mobile")) e.Mobile = usermodels.Mobile;
                if (usermodels.Birthday != null && ModelState.IsValidField("Birthday")) e.Birthday = usermodels.Birthday;
                if (usermodels.EntryDate != null && ModelState.IsValidField("EntryDate")) e.EntryDate = usermodels.EntryDate;
                if (usermodels.Position != null && ModelState.IsValidField("Position")) e.Position = usermodels.Position;
                if (usermodels.PoliticalStatus != null && ModelState.IsValidField("PoliticalStatus")) e.PoliticalStatus = usermodels.PoliticalStatus;
                if (usermodels.Address != null && ModelState.IsValidField("Address")) e.Address = usermodels.Address;
                if (usermodels.Skill != null && ModelState.IsValidField("Skill")) e.Skill = usermodels.Skill;
                if (usermodels.Experience != null && ModelState.IsValidField("Experience")) e.Experience = usermodels.Experience;
                if (usermodels.Remark != null && ModelState.IsValidField("Remark")) e.Remark = usermodels.Remark;

                if (db.Entry(e).State == EntityState.Modified)
                {
                    e.Changer = User.Identity.Name;
                    e.ChangeTime = DateTime.Now;
                    int x = await db.SaveChangesAsync();
                    if (x != 0)
                    {
                        changed = true;
                        ar.Operator = "Update";
                        db.ArUsers.Add(ar);
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
                return View(new UserModels());
            }
        }


        // GET: /User/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserModels usermodels = await db.Users.FindAsync(id);
            if (usermodels == null)
            {
                return HttpNotFound();
            }
            return View(usermodels);
        }

        // POST: /User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UserModels toDelete = await db.Users.FindAsync(id);
            db.Users.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ArUserModels ar = new ArUserModels(toDelete);
                ar.Operator = "Delete";
                db.ArUsers.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /User/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<UserModels> l = getQuery();
            List<UserModels> list = getSelected(l);
            foreach (UserModels e in list)
            {
                db.Users.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ArUserModels ar = new ArUserModels(e);
                    ar.Operator = "Delete";
                    db.ArUsers.Add(ar);
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

        // POST: /Equipment/FileUpload
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult FileUpload(HttpPostedFileBase[] photos)
        {
            string key = Request.Form["key"];
            string fullname = "";

            foreach (HttpPostedFileBase file in photos)
            {
                if (file != null)
                {
                    string filePath = Path.Combine((AppDomain.CurrentDomain.BaseDirectory + @"img\user\"), Path.GetFileName(file.FileName));
                    file.SaveAs(filePath);
                    fullname += "$" + file.FileName;
                }
            }

            UserModels e = db.Users.Find(key);
            // TODO - check e;
            e.Image = fullname;
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = key });

        }

        // GET: /User/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<UserModels> list = db.Users.ToList();

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
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "人员信息.xls");
        }
        
    }
}
