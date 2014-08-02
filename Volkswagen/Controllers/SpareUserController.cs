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
    public class SpareUserController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /SpareUser/
        public async Task<ActionResult> Index()
        {
            var spareusers = db.SpareUsers.Include(s => s.Spares);
            return View(await spareusers.ToListAsync());
        }

        // GET: /SpareUser/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareUserModels spareusermodels = await db.SpareUsers.FindAsync(id);
            if (spareusermodels == null)
            {
                return HttpNotFound();
            }
            return View(spareusermodels);
        }

        // GET: /SpareUser/Create
        public ActionResult Create()
        {
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareDes");
            return View();
        }

        // POST: /SpareUser/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="UserID,SpareID,SpareDes,Type,InValue,OutValue,User,UseTime,ActualUse,ChangeTime,Changer,CreateTime,Creator")] SpareUserModels spareusermodels)
        {
            if (ModelState.IsValid)
            {
                db.SpareUsers.Add(spareusermodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareDes", spareusermodels.SpareID);
            return View(spareusermodels);
        }

        // GET: /SpareUser/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareUserModels spareusermodels = await db.SpareUsers.FindAsync(id);
            if (spareusermodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareDes", spareusermodels.SpareID);
            return View(spareusermodels);
        }

        // POST: /SpareUser/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="UserID,SpareID,SpareDes,Type,InValue,OutValue,User,UseTime,ActualUse,ChangeTime,Changer,CreateTime,Creator")] SpareUserModels spareusermodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(spareusermodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareDes", spareusermodels.SpareID);
            return View(spareusermodels);
        }

        // GET: /SpareUser/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareUserModels spareusermodels = await db.SpareUsers.FindAsync(id);
            if (spareusermodels == null)
            {
                return HttpNotFound();
            }
            return View(spareusermodels);
        }

        // POST: /SpareUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SpareUserModels spareusermodels = await db.SpareUsers.FindAsync(id);
            db.SpareUsers.Remove(spareusermodels);
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
