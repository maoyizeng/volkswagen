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
    public class EquipLogController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /EquipLog/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            IQueryable<EquipLogModels> list = db.EquipLogs.Where("1 = 1");
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
                return View(await db.EquipLogs.ToListAsync());
            }
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> Index()
        {
            ParameterExpression param = Expression.Parameter(typeof(EquipLogModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++)
            {
                string field = Request.Form["field" + n];
                string op = Request.Form["op" + n];
                string operand = Request.Form["operand" + n];
                if (string.IsNullOrEmpty(field)) break;

                Expression left = Expression.Property(param, typeof(EquipLogModels).GetProperty(field));
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

            var e = db.EquipLogs;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(EquipLogModels) }, Expression.Constant(e), pred);

            IQueryable<EquipLogModels> list = db.EquipLogs.AsQueryable().Provider.CreateQuery<EquipLogModels>(expr);


            return View(list);
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
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes");
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
                db.EquipLogs.Add(equiplogmodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", equiplogmodels.EquipmentID);
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
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", equiplogmodels.EquipmentID);
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
                db.Entry(equiplogmodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", equiplogmodels.EquipmentID);
            return View(equiplogmodels);
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
            EquipLogModels equiplogmodels = await db.EquipLogs.FindAsync(id);
            db.EquipLogs.Remove(equiplogmodels);
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
