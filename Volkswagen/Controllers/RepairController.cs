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
    public class RepairController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Repair/
        /*public async Task<ActionResult> Index(GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<RepairModels> list = db.Repairs.Where("1 = 1");
            if (!string.IsNullOrEmpty(model.Column))
            {
                list = list.OrderBy(model.Column);
            }
            else
            {
                return View(await db.Repairs.ToListAsync());
            }
            return View(list);
        }*/

        // GET: /Repair/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            IQueryable<RepairModels> list = db.Repairs.Where("1 = 1");
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
                return View(await db.Repairs.ToListAsync());
            }
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> Index()
        {
            ParameterExpression param = Expression.Parameter(typeof(RepairModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++)
            {
                string field = Request.Form["field" + n];
                string op = Request.Form["op" + n];
                string operand = Request.Form["operand" + n];
                if (string.IsNullOrEmpty(field)) break;

                Expression left = Expression.Property(param, typeof(RepairModels).GetProperty(field));
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

            var e = db.Repairs;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(RepairModels) }, Expression.Constant(e), pred);

            IQueryable<RepairModels> list = db.Repairs.AsQueryable().Provider.CreateQuery<RepairModels>(expr);


            return View(list);
        }

        // GET: /Repair/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RepairModels repairmodels = await db.Repairs.FindAsync(id);
            if (repairmodels == null)
            {
                return HttpNotFound();
            }
            return View(repairmodels);
        }

        // GET: /Repair/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes");
            return View();
        }

        // POST: /Repair/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="SheetID,RepairID,EquipmentID,EquipDes,StartTime,FinishTime,RepairTime,Class,Line,Section,FaultView,Repairman,Description,FaultType,Result,Problem,Checker,Remark,StopTime,File,RepairNum,ChangeTime,Changer,CreateTime,Creator")] RepairModels repairmodels)
        {
            if (ModelState.IsValid)
            {
                db.Repairs.Add(repairmodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", repairmodels.EquipmentID);
            return View(repairmodels);
        }

        // GET: /Repair/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RepairModels repairmodels = await db.Repairs.FindAsync(id);
            if (repairmodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", repairmodels.EquipmentID);
            return View(repairmodels);
        }

        // POST: /Repair/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="SheetID,RepairID,EquipmentID,EquipDes,StartTime,FinishTime,RepairTime,Class,Line,Section,FaultView,Repairman,Description,FaultType,Result,Problem,Checker,Remark,StopTime,File,RepairNum,ChangeTime,Changer,CreateTime,Creator")] RepairModels repairmodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(repairmodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", repairmodels.EquipmentID);
            return View(repairmodels);
        }

        // POST: /Repair/Query
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Query()
        {

            ParameterExpression param = Expression.Parameter(typeof(RepairModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++)
            {
                string field = Request.Form["field" + n];
                string op = Request.Form["op" + n];
                string operand = Request.Form["operand" + n];
                if (string.IsNullOrEmpty(field)) break;

                Expression left = Expression.Property(param, typeof(RepairModels).GetProperty(field));
                Expression right = Expression.Constant(operand);
                Expression result;

                switch (Convert.ToByte(op))
                {
                    case 0:
                        result = Expression.Equal(left, right);
                        break;
                    case 1:
                        result = Expression.GreaterThan(left, right);
                        break;
                    case 2:
                        result = Expression.LessThan(left, right);
                        break;
                    case 3:
                        result = Expression.GreaterThanOrEqual(left, right);
                        break;
                    case 4:
                        result = Expression.LessThanOrEqual(left, right);
                        break;
                    case 5:
                        result = Expression.Equal(left, right);
                        break;
                    default:
                        result = Expression.Equal(left, right);
                        break;
                }
                filter = Expression.And(filter, result);
            }

            Expression pred = Expression.Lambda(filter, param);

            var e = db.Repairs;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(RepairModels) }, Expression.Constant(e), pred);

            ViewData.Model = db.Repairs.AsQueryable().Provider.CreateQuery<RepairModels>(expr).ToList();
            return RedirectToAction("Index");
        }

        // GET: /Repair/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RepairModels repairmodels = await db.Repairs.FindAsync(id);
            if (repairmodels == null)
            {
                return HttpNotFound();
            }
            return View(repairmodels);
        }

        // POST: /Repair/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            RepairModels repairmodels = await db.Repairs.FindAsync(id);
            db.Repairs.Remove(repairmodels);
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

        // GET: /Repair/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<RepairModels> list = db.Repairs.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "原单据编号",
        "保修单号",
        "设备名称",
        "设备编号",
        "保修时刻",
        "修复时刻",
        "维修耗时",
        "班次",
        "车间生产线",
        "工段",
        "故障现象",
        "维修人",
        "故障原因和维修内容",
        "故障类别",
        "已修复否",
        "遗留问题",
        "验收人",
        "备注",
        "停机时间",
        "维修次数",
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.SheetID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.RepairID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipDes);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.StartTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.FinishTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.RepairTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Class);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Line);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Section);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.FaultView);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Repairman);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Description);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.FaultType);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Result);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Problem);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Checker);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Remark);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.StopTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.RepairNum);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ChangeTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Changer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CreateTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Creator);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "设备报修单.xls");
        }
    }
}
