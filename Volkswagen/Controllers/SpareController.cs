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
    public class SpareController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Spare/
        public async Task<ActionResult> Index()
        {
            var spares = db.Spares.Include(s => s.Equipments);
            return View(await spares.ToListAsync());
        }

        // GET: /Spare/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareModels sparemodels = await db.Spares.FindAsync(id);
            if (sparemodels == null)
            {
                return HttpNotFound();
            }
            return View(sparemodels);
        }

        // GET: /Spare/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes");
            return View();
        }

        // POST: /Spare/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="SpareID,SpareDes,Type,Picture1,Picture2,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,File,ChangeTime,Changer,CreateTime,Creator")] SpareModels sparemodels)
        {
            if (ModelState.IsValid)
            {
                db.Spares.Add(sparemodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", sparemodels.EquipmentID);
            return View(sparemodels);
        }

        // GET: /Spare/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareModels sparemodels = await db.Spares.FindAsync(id);
            if (sparemodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", sparemodels.EquipmentID);
            return View(sparemodels);
        }

        // POST: /Spare/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="SpareID,SpareDes,Type,Picture1,Picture2,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,File,ChangeTime,Changer,CreateTime,Creator")] SpareModels sparemodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sparemodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes", sparemodels.EquipmentID);
            return View(sparemodels);
        }

        // GET: /Spare/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareModels sparemodels = await db.Spares.FindAsync(id);
            if (sparemodels == null)
            {
                return HttpNotFound();
            }
            return View(sparemodels);
        }

        // POST: /Spare/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            SpareModels sparemodels = await db.Spares.FindAsync(id);
            db.Spares.Remove(sparemodels);
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
