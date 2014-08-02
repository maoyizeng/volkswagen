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
    public class SpareOrderController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /SpareOrder/
        public async Task<ActionResult> Index()
        {
            var spareorders = db.SpareOrders.Include(s => s.Equipments).Include(s => s.Spares);
            return View(await spareorders.ToListAsync());
        }

        // GET: /SpareOrder/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareOrderModels spareordermodels = await db.SpareOrders.FindAsync(id);
            if (spareordermodels == null)
            {
                return HttpNotFound();
            }
            return View(spareordermodels);
        }

        // GET: /SpareOrder/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes");
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareDes");
            return View();
        }

        // POST: /SpareOrder/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="OrderID,SpareID,SpareDes,Type,OrderValue,Producer,OrderNum,Property,EquipmentID,Maker,MakerNum,Orderman,UseTime,UnitPrice,TotalPrice,Status,Mode,OrderFile,ChangeTime,Changer,CreateTime,Creator")] SpareOrderModels spareordermodels)
        {
            if (ModelState.IsValid)
            {
                db.SpareOrders.Add(spareordermodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", spareordermodels.EquipmentID);
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareDes", spareordermodels.SpareID);
            return View(spareordermodels);
        }

        // GET: /SpareOrder/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareOrderModels spareordermodels = await db.SpareOrders.FindAsync(id);
            if (spareordermodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", spareordermodels.EquipmentID);
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareDes", spareordermodels.SpareID);
            return View(spareordermodels);
        }

        // POST: /SpareOrder/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="OrderID,SpareID,SpareDes,Type,OrderValue,Producer,OrderNum,Property,EquipmentID,Maker,MakerNum,Orderman,UseTime,UnitPrice,TotalPrice,Status,Mode,OrderFile,ChangeTime,Changer,CreateTime,Creator")] SpareOrderModels spareordermodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(spareordermodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", spareordermodels.EquipmentID);
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareDes", spareordermodels.SpareID);
            return View(spareordermodels);
        }

        // GET: /SpareOrder/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareOrderModels spareordermodels = await db.SpareOrders.FindAsync(id);
            if (spareordermodels == null)
            {
                return HttpNotFound();
            }
            return View(spareordermodels);
        }

        // POST: /SpareOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SpareOrderModels spareordermodels = await db.SpareOrders.FindAsync(id);
            db.SpareOrders.Remove(spareordermodels);
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
