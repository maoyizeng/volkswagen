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

namespace Volkswagen.ArControllers
{
    public class ArRepairController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArRepair/
        public async Task<ActionResult> Index()
        {
            return View(await db.ArRepairs.ToListAsync());
        }

        // GET: /ArRepair/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArRepairModels arrepairmodels = await db.ArRepairs.FindAsync(id);
            if (arrepairmodels == null)
            {
                return HttpNotFound();
            }
            return View(arrepairmodels);
        }

        // GET: /ArRepair/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /ArRepair/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="SheetID,Operator,OperateTime,RepairID,EquipmentID,EquipDes,StartTime,FinishTime,RepairTime,Class,Line,Section,FaultView,Repairman,Description,FaultType,Result,Problem,Checker,Remark,StopTime,RepairNum,ChangeTime,Changer,CreateTime,Creator")] ArRepairModels arrepairmodels)
        {
            if (ModelState.IsValid)
            {
                db.ArRepairs.Add(arrepairmodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(arrepairmodels);
        }

        // GET: /ArRepair/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArRepairModels arrepairmodels = await db.ArRepairs.FindAsync(id);
            if (arrepairmodels == null)
            {
                return HttpNotFound();
            }
            return View(arrepairmodels);
        }

        // POST: /ArRepair/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="SheetID,Operator,OperateTime,RepairID,EquipmentID,EquipDes,StartTime,FinishTime,RepairTime,Class,Line,Section,FaultView,Repairman,Description,FaultType,Result,Problem,Checker,Remark,StopTime,RepairNum,ChangeTime,Changer,CreateTime,Creator")] ArRepairModels arrepairmodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(arrepairmodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(arrepairmodels);
        }

        // GET: /ArRepair/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArRepairModels arrepairmodels = await db.ArRepairs.FindAsync(id);
            if (arrepairmodels == null)
            {
                return HttpNotFound();
            }
            return View(arrepairmodels);
        }

        // POST: /ArRepair/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            ArRepairModels arrepairmodels = await db.ArRepairs.FindAsync(id);
            db.ArRepairs.Remove(arrepairmodels);
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
