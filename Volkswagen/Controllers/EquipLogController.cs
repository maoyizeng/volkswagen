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
    public class EquipLogController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /EquipLog/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<EquipLogModels> list = getQuery(false);
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

            IQueryable<EquipLogModels> list = getQuery();

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

        private IQueryable<EquipLogModels> getQuery(bool post = true)
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(EquipLogModels), "p");
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
                Expression left = Expression.Property(param, typeof(EquipLogModels).GetProperty(field));
                //[operandn]
                Expression right = Expression.Constant(operand);
                Expression result;

                switch (field)
                {
                    case "ChangeTime":
                    case "CreateTime":
                    case "DocumentDate":
                    case "EnableDate":
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "OriginValue":
                    case "Depreciation":
                        right = Expression.Constant(decimal.Parse(operand));
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
            var e = db.EquipLogs;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(EquipLogModels) }, Expression.Constant(e), pred);

            IQueryable<EquipLogModels> list = db.EquipLogs.AsQueryable().Provider.CreateQuery<EquipLogModels>(expr);

            return list;
        }

        private List<EquipLogModels> getSelected(IQueryable<EquipLogModels> l)
        {
            List<EquipLogModels> list = new List<EquipLogModels>();
            List<EquipLogModels> list_origin = l.ToList();
            foreach (EquipLogModels e in list_origin)
            {
                if (Request.Form["Checked" + e.EquipmentID] != "false")
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /EquipLog/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipLogModels equiplogmodels = await db.EquipLogs.FindAsync(id);
            if (equiplogmodels == null)
            {
                return HttpNotFound();
            }
            return View(equiplogmodels);
        }

        // GET: /EquipLog/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
            return View();
        }

        // POST: /EquipLog/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="EquipmentID,Department,EquipDes,Type,Spec,DocumentDate,EnableDate,OriginValue,Depreciation,Spot1,Spot2,Spot3,Remark,ChangeTime,Changer,CreateTime,Creator")] EquipLogModels equiplogmodels)
        {
            if (ModelState.IsValid)
            {
                equiplogmodels.Changer = User.Identity.Name;
                equiplogmodels.Creator = User.Identity.Name;
                equiplogmodels.CreateTime = DateTime.Now;
                equiplogmodels.ChangeTime = DateTime.Now;
                db.EquipLogs.Add(equiplogmodels);

                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ArEquipLogModels ar = new ArEquipLogModels(equiplogmodels);
                    ar.Operator = "Create";
                    db.ArEquipLogs.Add(ar);
                    await db.SaveChangesAsync();
                }
             
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", equiplogmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", equiplogmodels.EquipDes);
            return View(equiplogmodels);
        }

        // GET: /EquipLog/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipLogModels equiplogmodels = await db.EquipLogs.FindAsync(id);
            if (equiplogmodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", equiplogmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", equiplogmodels.EquipDes); 
            return View(equiplogmodels);
        }

        // POST: /EquipLog/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="EquipmentID,Department,EquipDes,Type,Spec,DocumentDate,EnableDate,OriginValue,Depreciation,Spot1,Spot2,Spot3,Remark,ChangeTime,Changer,CreateTime,Creator")] EquipLogModels equiplogmodels)
        {
            if (ModelState.IsValid)
            {
                var toUpdate = db.EquipLogs.Find(equiplogmodels.EquipmentID);

                equiplogmodels.Changer = User.Identity.Name;
                equiplogmodels.ChangeTime = DateTime.Now;
                equiplogmodels.Creator = toUpdate.Creator;
                equiplogmodels.CreateTime = toUpdate.CreateTime;


                db.Entry(toUpdate).State = EntityState.Detached;
                db.Entry(equiplogmodels).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArEquipLogModels ar = new ArEquipLogModels(toUpdate);
                    ar.Operator = "Update";
                    db.ArEquipLogs.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", equiplogmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", equiplogmodels.EquipDes); 
            return View(equiplogmodels);
        }

        // POST: /EquipLog/EditMultiple/
        //[HttpPost]
        public async Task<ActionResult> EditMultiple()
        {
            IQueryable<EquipLogModels> l = getQuery();
            List<EquipLogModels> list = getSelected(l);
            if (ViewData["list"] == null) ViewData["list"] = list;
            //string key = list.First().EquipLogID;
            //return RedirectToAction("Edit", new { id = key });
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
            return RedirectToAction("ChangeMultiple", new { EquipLogmodels = new EquipLogModels() });
        }

        // POST: /EquipLog/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "EquipmentID,Department,EquipDes,Type,Spec,DocumentDate,EnableDate,OriginValue,Depreciation,Spot1,Spot2,Spot3,Remark,ChangeTime,Changer,CreateTime,Creator")] EquipLogModels equiplogmodels)
        {
            bool changed = false;
            List<EquipLogModels> l = new List<EquipLogModels>();
            for (int i = 0; ; i++)
            {
                string id = Request.Form["item" + i];
                if (Request.Form["item" + i] == null) break;
                EquipLogModels e = db.EquipLogs.Find(id);
                l.Add(e);
                ArEquipLogModels ar = new ArEquipLogModels(e);
                if (equiplogmodels.EquipmentID != null && ModelState.IsValidField("EquipmentID")) e.EquipmentID = equiplogmodels.EquipmentID;
                if (equiplogmodels.EquipDes != null && ModelState.IsValidField("EquipDes")) e.EquipDes = equiplogmodels.EquipDes;
                if (equiplogmodels.Department != null && ModelState.IsValidField("Department")) e.Department = equiplogmodels.Department;
                if (equiplogmodels.Type != null && ModelState.IsValidField("Type")) e.Type = equiplogmodels.Type;
                if (equiplogmodels.Spec != null && ModelState.IsValidField("Spec")) e.Spec = equiplogmodels.Spec;
                if (equiplogmodels.DocumentDate != null && ModelState.IsValidField("DocumentDate")) e.DocumentDate = equiplogmodels.DocumentDate;
                if (equiplogmodels.EnableDate != null && ModelState.IsValidField("EnableDate")) e.EnableDate = equiplogmodels.EnableDate;
                if (equiplogmodels.OriginValue != null && ModelState.IsValidField("OriginValue")) e.OriginValue = equiplogmodels.OriginValue;
                if (equiplogmodels.Depreciation != null && ModelState.IsValidField("Depreciation")) e.Depreciation = equiplogmodels.Depreciation;
                if (equiplogmodels.Spot1 != null && ModelState.IsValidField("Spot1")) e.Spot1 = equiplogmodels.Spot1;
                if (equiplogmodels.Spot2 != null && ModelState.IsValidField("Spot2")) e.Spot2 = equiplogmodels.Spot2;
                if (equiplogmodels.Spot3 != null && ModelState.IsValidField("Spot3")) e.Spot3 = equiplogmodels.Spot3;
                if (equiplogmodels.Remark != null && ModelState.IsValidField("Remark")) e.Remark = equiplogmodels.Remark;

                if (db.Entry(e).State == EntityState.Modified)
                {
                    e.Changer = User.Identity.Name;
                    e.ChangeTime = DateTime.Now;
                    int x = await db.SaveChangesAsync();
                    if (x != 0)
                    {
                        changed = true;
                        ar.Operator = "Update";
                        db.ArEquipLogs.Add(ar);
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
                ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
                ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
                return View(new EquipLogModels());
            }
        }


        // GET: /EquipLog/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipLogModels equiplogmodels = await db.EquipLogs.FindAsync(id);
            if (equiplogmodels == null)
            {
                return HttpNotFound();
            }
            return View(equiplogmodels);
        }

        // POST: /EquipLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            EquipLogModels toDelete = await db.EquipLogs.FindAsync(id);
            db.EquipLogs.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ArEquipLogModels ar = new ArEquipLogModels(toDelete);
                ar.Operator = "Delete";
                db.ArEquipLogs.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /EquipLog/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple()
        {
            IQueryable<EquipLogModels> l = getQuery();
            List<EquipLogModels> list = getSelected(l);
            foreach (EquipLogModels e in list)
            {
                db.EquipLogs.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ArEquipLogModels ar = new ArEquipLogModels(e);
                    ar.Operator = "Delete";
                    db.ArEquipLogs.Add(ar);
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

        // GET: /EquipLog/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<EquipLogModels> list = db.EquipLogs.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
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
        "创建人" };
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            foreach (var i in list)
            {
                sbHtml.Append("<tr>");
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
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "设备台账.xls");
        }
    }
}
