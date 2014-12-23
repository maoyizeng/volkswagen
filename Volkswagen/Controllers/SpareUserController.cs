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
using MvcContrib.Pagination;
using System.Text;

namespace Volkswagen.Controllers
{
    public class SpareUserController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /SpareUser/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<SpareUserModels> list = getQuery(false);
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

            IQueryable<SpareUserModels> list = getQuery();

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

        private IQueryable<SpareUserModels> getQuery(bool post = true)
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(SpareUserModels), "p");
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
                Expression left = Expression.Property(param, typeof(SpareUserModels).GetProperty(field));
                //[operandn]
                Expression right = Expression.Constant(operand);
                Expression result;

                switch (field)
                {
                    case "ChangeTime":
                    case "CreateTime":
                    case "UseTime":
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "InValue":
                    case "OutValue":
                    case "UserID":
                        right = Expression.Constant(int.Parse(operand));
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
            var e = db.SpareUsers;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(SpareUserModels) }, Expression.Constant(e), pred);

            IQueryable<SpareUserModels> list = db.SpareUsers.AsQueryable().Provider.CreateQuery<SpareUserModels>(expr);

            return list;
        }

        private List<SpareUserModels> getSelected(IQueryable<SpareUserModels> l)
        {
            List<SpareUserModels> list = new List<SpareUserModels>();
            List<SpareUserModels> list_origin = l.ToList();
            foreach (SpareUserModels e in list_origin)
            {
                if (Request.Form["Checked" + e.UserID] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

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
        public async Task<ActionResult> Create([Bind(Include="SpareID,SpareDes,Type,InValue,OutValue,User,UseTime,ActualUse,ChangeTime,Changer,CreateTime,Creator")] SpareUserModels spareusermodels)
        {
            if (db.Spares.Find(spareusermodels.SpareID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(spareusermodels);
            }

            if (ModelState.IsValid)
            {
                spareusermodels.Changer = User.Identity.Name;
                spareusermodels.Creator = User.Identity.Name;
                spareusermodels.CreateTime = DateTime.Now;
                spareusermodels.ChangeTime = DateTime.Now;
                db.SpareUsers.Add(spareusermodels);

                var sp = db.Spares.Find(spareusermodels.SpareID);
                ArSpareModels arsp = new ArSpareModels(sp);

                if (spareusermodels.InValue != null)
                {
                    sp.PresentValue += spareusermodels.InValue;
                }
                if (spareusermodels.OutValue != null)
                {
                    sp.PresentValue -= spareusermodels.OutValue;
                }
                db.Entry(sp).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    var new_ins = db.SpareUsers.OrderByDescending(p => p.UserID).First();
                    ArSpareUserModels ar = new ArSpareUserModels(new_ins);
                    ar.Operator = ArEquipmentModels.OperatorType.创建;
                    db.ArSpareUsers.Add(ar);
                    
                    arsp.Operator = ArEquipmentModels.OperatorType.修改;
                    db.ArSpares.Add(arsp);

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
            if (db.Spares.Find(spareusermodels.SpareID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(spareusermodels);
            }
            if (ModelState.IsValid)
            {
                var toUpdate = db.SpareUsers.Find(spareusermodels.UserID);

                spareusermodels.Changer = User.Identity.Name;
                spareusermodels.ChangeTime = DateTime.Now;
                spareusermodels.Creator = toUpdate.Creator;
                spareusermodels.CreateTime = toUpdate.CreateTime;

                db.Entry(toUpdate).State = EntityState.Detached;
                db.Entry(spareusermodels).State = EntityState.Modified;

                var sp = db.Spares.Find(spareusermodels.SpareID);
                ArSpareModels arsp = new ArSpareModels(sp);
                if (spareusermodels.InValue != null)
                {
                    sp.PresentValue += spareusermodels.InValue;
                }
                if (spareusermodels.OutValue != null)
                {
                    sp.PresentValue -= spareusermodels.OutValue;
                }
                db.Entry(sp).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArSpareUserModels ar = new ArSpareUserModels(toUpdate);
                    ar.Operator = ArEquipmentModels.OperatorType.修改;
                    db.ArSpareUsers.Add(ar);

                    
                    arsp.Operator = ArEquipmentModels.OperatorType.修改;
                    db.ArSpares.Add(arsp);

                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID", spareusermodels.SpareID);
            ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes", spareusermodels.SpareDes);
            return View(spareusermodels);
        }

        // POST: /SpareUser/EditMultiple/
        //[HttpPost]
        public async Task<ActionResult> EditMultiple()
        {
            IQueryable<SpareUserModels> l = getQuery();
            List<SpareUserModels> list = getSelected(l);
            if (ViewData["list"] == null) ViewData["list"] = list;
            //string key = list.First().SpareUserID;
            //return RedirectToAction("Edit", new { id = key });
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID");
            ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes");
            return RedirectToAction("ChangeMultiple", new { SpareUsermodels = new SpareUserModels() });
        }

        // POST: /SpareUser/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "UserID,SpareID,SpareDes,Type,InValue,OutValue,User,UseTime,ActualUse,ChangeTime,Changer,CreateTime,Creator")] SpareUserModels spareusermodels)
        {
            if (db.Spares.Find(spareusermodels.SpareID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(spareusermodels);
            }

            bool changed = false;
            List<SpareUserModels> l = new List<SpareUserModels>();
            for (int i = 0; ; i++)
            {
                string id = Request.Form["item" + i];
                if (Request.Form["item" + i] == null) break;
                SpareUserModels e = db.SpareUsers.Find(id);
                l.Add(e);
                ArSpareUserModels ar = new ArSpareUserModels(e);
                if (spareusermodels.SpareID != null && ModelState.IsValidField("SpareID")) e.SpareID = spareusermodels.SpareID;
                if (spareusermodels.SpareDes != null && ModelState.IsValidField("SpareDes")) e.SpareDes = spareusermodels.SpareDes;
                if (spareusermodels.Type != null && ModelState.IsValidField("Type")) e.Type = spareusermodels.Type;
                if (spareusermodels.InValue != null && ModelState.IsValidField("InValue")) e.InValue = spareusermodels.InValue;
                if (spareusermodels.OutValue != null && ModelState.IsValidField("OutValue")) e.OutValue = spareusermodels.OutValue;
                if (spareusermodels.User != null && ModelState.IsValidField("User")) e.User = spareusermodels.User;
                if (spareusermodels.UseTime != null && ModelState.IsValidField("UseTime")) e.UseTime = spareusermodels.UseTime;
                if (spareusermodels.ActualUse != null && ModelState.IsValidField("ActualUse")) e.ActualUse = spareusermodels.ActualUse;

                if (db.Entry(e).State == EntityState.Modified)
                {
                    e.Changer = User.Identity.Name;
                    e.ChangeTime = DateTime.Now;
                    int x = await db.SaveChangesAsync();
                    if (x != 0)
                    {
                        changed = true;
                        ar.Operator = ArEquipmentModels.OperatorType.修改;
                        db.ArSpareUsers.Add(ar);
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
                ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID");
                ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes"); 
                return View(new SpareUserModels());
            }
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
            ArSpareUserModels ar = new ArSpareUserModels(toDelete);
            db.SpareUsers.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.删除;
                db.ArSpareUsers.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /SpareUser/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<SpareUserModels> l = getQuery();
            List<SpareUserModels> list = getSelected(l);
            foreach (SpareUserModels e in list)
            {
                ArSpareUserModels ar = new ArSpareUserModels(e);
                db.SpareUsers.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ar.Operator = ArEquipmentModels.OperatorType.删除;
                    db.ArSpareUsers.Add(ar);
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

        // GET: /SpareUser/ExportExcel
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
