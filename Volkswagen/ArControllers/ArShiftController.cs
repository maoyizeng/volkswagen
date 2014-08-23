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

namespace Volkswagen.ArControllers
{
    public class ArShiftController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArShift/
        public async Task<ActionResult> Index()
        {
            return View(await db.ArShifts.ToListAsync());
        }

        // GET: /ArShift/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArShiftModels arshiftmodels = await db.ArShifts.FindAsync(id);
            if (arshiftmodels == null)
            {
                return HttpNotFound();
            }
            return View(arshiftmodels);
        }

        // GET: /ArShift/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /ArShift/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="ShiftID,Operator,OperateTime,ShiftDate,ShiftTime,Class,Line,Charger,Record,Urgency,Remark,ChangeTime,Changer,CreateTime,Creator")] ArShiftModels arshiftmodels)
        {
            if (ModelState.IsValid)
            {
                db.ArShifts.Add(arshiftmodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(arshiftmodels);
        }

        // GET: /ArShift/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArShiftModels arshiftmodels = await db.ArShifts.FindAsync(id);
            if (arshiftmodels == null)
            {
                return HttpNotFound();
            }
            return View(arshiftmodels);
        }

        // POST: /ArShift/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="ShiftID,Operator,OperateTime,ShiftDate,ShiftTime,Class,Line,Charger,Record,Urgency,Remark,ChangeTime,Changer,CreateTime,Creator")] ArShiftModels arshiftmodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(arshiftmodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(arshiftmodels);
        }

        // GET: /ArShift/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArShiftModels arshiftmodels = await db.ArShifts.FindAsync(id);
            if (arshiftmodels == null)
            {
                return HttpNotFound();
            }
            return View(arshiftmodels);
        }

        // POST: /ArShift/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            ArShiftModels arshiftmodels = await db.ArShifts.FindAsync(id);
            db.ArShifts.Remove(arshiftmodels);
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
