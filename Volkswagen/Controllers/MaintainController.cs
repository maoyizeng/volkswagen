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
    public class MaintainController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Maintain/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            IQueryable<MaintainModels> list = db.Maintains.Where("1 = 1");
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
                return View(await db.Maintains.ToListAsync());
            }
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> Index()
        {
            ParameterExpression param = Expression.Parameter(typeof(MaintainModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++)
            {
                string field = Request.Form["field" + n];
                string op = Request.Form["op" + n];
                string operand = Request.Form["operand" + n];
                if (string.IsNullOrEmpty(field)) break;

                Expression left = Expression.Property(param, typeof(MaintainModels).GetProperty(field));
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

            var e = db.Maintains;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(MaintainModels) }, Expression.Constant(e), pred);

            IQueryable<MaintainModels> list = db.Maintains.AsQueryable().Provider.CreateQuery<MaintainModels>(expr);
            
            return View(list);
        } 

        // GET: /Maintain/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaintainModels maintainmodels = await db.Maintains.FindAsync(id);
            if (maintainmodels == null)
            {
                return HttpNotFound();
            }
            return View(maintainmodels);
        }

        // GET: /Maintain/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
            return View();
        }

        // POST: /Maintain/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="MaintainId,EquipmentID,EquipDes,Line,MType,MPart,Content,Period,MStartTime,MEndTime,ResponseClass,CheckStatus,CheckDetail,EquipStatus,EquipDetail,CheckerType,Checker,CheckTime,Problem,Mark,Grade,ProblemStatus,CheckNum,ChangeTime,Changer,CreateTime,Creator")] MaintainModels maintainmodels)
        {
            if (ModelState.IsValid)
            {
                maintainmodels.Changer = User.Identity.Name;
                maintainmodels.Creator = User.Identity.Name;
                maintainmodels.CreateTime = DateTime.Now;
                maintainmodels.ChangeTime = DateTime.Now;
                ArMaintainModels ar = new ArMaintainModels(maintainmodels);
                ar.Operator = "Create";

                db.ArMaintains.Add(ar);
                db.Maintains.Add(maintainmodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", maintainmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", maintainmodels.EquipDes);
            return View(maintainmodels);
        }

        // GET: /Maintain/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaintainModels maintainmodels = await db.Maintains.FindAsync(id);
            if (maintainmodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", maintainmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", maintainmodels.EquipDes);
            return View(maintainmodels);
        }

        // POST: /Maintain/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="MaintainId,EquipmentID,EquipDes,Line,MType,MPart,Content,Period,MStartTime,MEndTime,ResponseClass,CheckStatus,CheckDetail,EquipStatus,EquipDetail,CheckerType,Checker,CheckTime,Problem,Mark,Grade,ProblemStatus,CheckNum,ChangeTime,Changer,CreateTime,Creator")] MaintainModels maintainmodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(maintainmodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", maintainmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", maintainmodels.EquipDes);
            return View(maintainmodels);
        }

        // GET: /Maintain/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaintainModels maintainmodels = await db.Maintains.FindAsync(id);
            if (maintainmodels == null)
            {
                return HttpNotFound();
            }
            return View(maintainmodels);
        }

        // POST: /Maintain/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            MaintainModels maintainmodels = await db.Maintains.FindAsync(id);
            db.Maintains.Remove(maintainmodels);
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

        // GET: /Maintain/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<MaintainModels> list = db.Maintains.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "设备编号",
                "设备名称",
                "维护类别",
                "保养部件",
                "保养位置",
                "保养内容",
                "保养周期",
                "注意事项",
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

            foreach (var i in list)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipDes);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Line);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MType);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MPart);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Content);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Period);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MStartTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MEndTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ResponseClass);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckStatus);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipDetail);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckerType);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Checker);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Problem);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Mark);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Grade);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ProblemStatus);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckNum);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MaintainId);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ChangeTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Changer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CreateTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Creator);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "设备保养计划.xls");
        }
    }
}
