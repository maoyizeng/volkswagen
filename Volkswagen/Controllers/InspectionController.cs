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

namespace Volkswagen.Controllers
{
    public class InspectionController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Inspection/
        public async Task<ActionResult> Index()
        {
            var inspections = db.Inspections.Include(i => i.Equipments);
            return View(await inspections.ToListAsync());
        }

        // GET: /Inspection/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InspectionModels inspectionmodels = await db.Inspections.FindAsync(id);
            if (inspectionmodels == null)
            {
                return HttpNotFound();
            }
            return View(inspectionmodels);
        }

        // GET: /Inspection/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes");
            return View();
        }

        // POST: /Inspection/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="InspectionId,EquipmentID,EquipDes,Class,Part,Position,Content,Period,Caution,Remark,YInspectionFile,MInspectionFile,OInspectionFile,OtherFile,ChangeTime,Changer,CreateTime,Creator")] InspectionModels inspectionmodels)
        {
            if (ModelState.IsValid)
            {
                db.Inspections.Add(inspectionmodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", inspectionmodels.EquipmentID);
            return View(inspectionmodels);
        }

        // GET: /Inspection/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InspectionModels inspectionmodels = await db.Inspections.FindAsync(id);
            if (inspectionmodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", inspectionmodels.EquipmentID);
            return View(inspectionmodels);
        }

        // POST: /Inspection/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="InspectionId,EquipmentID,EquipDes,Class,Part,Position,Content,Period,Caution,Remark,YInspectionFile,MInspectionFile,OInspectionFile,OtherFile,ChangeTime,Changer,CreateTime,Creator")] InspectionModels inspectionmodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inspectionmodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", inspectionmodels.EquipmentID);
            return View(inspectionmodels);
        }

        // GET: /Inspection/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InspectionModels inspectionmodels = await db.Inspections.FindAsync(id);
            if (inspectionmodels == null)
            {
                return HttpNotFound();
            }
            return View(inspectionmodels);
        }

        // POST: /Inspection/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            InspectionModels inspectionmodels = await db.Inspections.FindAsync(id);
            db.Inspections.Remove(inspectionmodels);
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
