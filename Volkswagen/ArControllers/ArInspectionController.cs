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
    public class ArInspectionController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArInspection/
        public async Task<ActionResult> Index()
        {
            return View(await db.ArInspections.ToListAsync());
        }

        // GET: /ArInspection/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArInspectionModels arinspectionmodels = await db.ArInspections.FindAsync(id);
            if (arinspectionmodels == null)
            {
                return HttpNotFound();
            }
            return View(arinspectionmodels);
        }

        // GET: /ArInspection/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /ArInspection/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="InspectionId,Operator,OperateTime,EquipmentID,EquipDes,Class,Part,Position,Content,Period,Caution,Remark,ChangeTime,Changer,CreateTime,Creator")] ArInspectionModels arinspectionmodels)
        {
            if (ModelState.IsValid)
            {
                db.ArInspections.Add(arinspectionmodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(arinspectionmodels);
        }

        // GET: /ArInspection/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArInspectionModels arinspectionmodels = await db.ArInspections.FindAsync(id);
            if (arinspectionmodels == null)
            {
                return HttpNotFound();
            }
            return View(arinspectionmodels);
        }

        // POST: /ArInspection/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="InspectionId,Operator,OperateTime,EquipmentID,EquipDes,Class,Part,Position,Content,Period,Caution,Remark,ChangeTime,Changer,CreateTime,Creator")] ArInspectionModels arinspectionmodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(arinspectionmodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(arinspectionmodels);
        }

        // GET: /ArInspection/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArInspectionModels arinspectionmodels = await db.ArInspections.FindAsync(id);
            if (arinspectionmodels == null)
            {
                return HttpNotFound();
            }
            return View(arinspectionmodels);
        }

        // POST: /ArInspection/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ArInspectionModels arinspectionmodels = await db.ArInspections.FindAsync(id);
            db.ArInspections.Remove(arinspectionmodels);
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
