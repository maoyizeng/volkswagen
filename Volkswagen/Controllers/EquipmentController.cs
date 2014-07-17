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
    public class EquipmentController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Equipment/
        public async Task<ActionResult> Index()
        {
            return View(await db.Equipments.ToListAsync());
        }

        // GET: /Equipment/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipmentModels equipmentmodels = await db.Equipments.FindAsync(id);
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // GET: /Equipment/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Equipment/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="EquipmentID,EquipDes,Person,Section,WSArea,Photo,ItemInspect,RegularCare,Check,RoutingInspect,TPMFile,Rules,TechnicFile,TrainingFile,ChangeTime,Changer,CreateTime,Creator,Remark")] EquipmentModels equipmentmodels)
        {
            if (ModelState.IsValid)
            {
                db.Equipments.Add(equipmentmodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(equipmentmodels);
        }

        // GET: /Equipment/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipmentModels equipmentmodels = await db.Equipments.FindAsync(id);
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // POST: /Equipment/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="EquipmentID,EquipDes,Person,Section,WSArea,Photo,ItemInspect,RegularCare,Check,RoutingInspect,TPMFile,Rules,TechnicFile,TrainingFile,ChangeTime,Changer,CreateTime,Creator,Remark")] EquipmentModels equipmentmodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(equipmentmodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(equipmentmodels);
        }

        // GET: /Equipment/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipmentModels equipmentmodels = await db.Equipments.FindAsync(id);
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // POST: /Equipment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            EquipmentModels equipmentmodels = await db.Equipments.FindAsync(id);
            db.Equipments.Remove(equipmentmodels);
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
