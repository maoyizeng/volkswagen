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

namespace Volkswagen.Controllers
{
    public class RepairController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Repair/
        public async Task<ActionResult> Index(GridSortOptions model)
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
    }
}
