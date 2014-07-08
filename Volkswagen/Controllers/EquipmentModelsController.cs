using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Volkswagen.Models;
using Volkswagen.DAL;

namespace Volkswagen.Controllers
{
    public class EquipmentModelsController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /EquipmentModels/
        public ActionResult Index()
        {
            return View(db.Equipments.ToList());
        }

        // GET: /EquipmentModels/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipmentModels equipmentmodels = db.Equipments.Find(id);
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // GET: /EquipmentModels/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /EquipmentModels/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="EquipmentNumber,EquipId,EquipDes,Person,Section,WSArea,Photo,ItemInspect,RegularCare,Check,RoutingInspect,TPMFile,Rules,TechnicFile,TrainingFile,ChangeTime,Changer,CreateTime,Creator,Remark")] EquipmentModels equipmentmodels)
        {
            if (ModelState.IsValid)
            {
                db.Equipments.Add(equipmentmodels);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(equipmentmodels);
        }

        // GET: /EquipmentModels/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipmentModels equipmentmodels = db.Equipments.Find(id);
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // POST: /EquipmentModels/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="EquipmentNumber,EquipId,EquipDes,Person,Section,WSArea,Photo,ItemInspect,RegularCare,Check,RoutingInspect,TPMFile,Rules,TechnicFile,TrainingFile,ChangeTime,Changer,CreateTime,Creator,Remark")] EquipmentModels equipmentmodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(equipmentmodels).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(equipmentmodels);
        }

        // GET: /EquipmentModels/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipmentModels equipmentmodels = db.Equipments.Find(id);
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // POST: /EquipmentModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            EquipmentModels equipmentmodels = db.Equipments.Find(id);
            db.Equipments.Remove(equipmentmodels);
            db.SaveChanges();
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
