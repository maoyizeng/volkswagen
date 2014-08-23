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
    public class ArSpareUserController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArSpareUser/
        public async Task<ActionResult> Index()
        {
            return View(await db.ArSpareUsers.ToListAsync());
        }

        // GET: /ArSpareUser/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareUserModels arspareusermodels = await db.ArSpareUsers.FindAsync(id);
            if (arspareusermodels == null)
            {
                return HttpNotFound();
            }
            return View(arspareusermodels);
        }

        // GET: /ArSpareUser/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /ArSpareUser/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="UserID,Operator,OperateTime,SpareID,SpareDes,Type,InValue,OutValue,User,UseTime,ActualUse,ChangeTime,Changer,CreateTime,Creator")] ArSpareUserModels arspareusermodels)
        {
            if (ModelState.IsValid)
            {
                db.ArSpareUsers.Add(arspareusermodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(arspareusermodels);
        }

        // GET: /ArSpareUser/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareUserModels arspareusermodels = await db.ArSpareUsers.FindAsync(id);
            if (arspareusermodels == null)
            {
                return HttpNotFound();
            }
            return View(arspareusermodels);
        }

        // POST: /ArSpareUser/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="UserID,Operator,OperateTime,SpareID,SpareDes,Type,InValue,OutValue,User,UseTime,ActualUse,ChangeTime,Changer,CreateTime,Creator")] ArSpareUserModels arspareusermodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(arspareusermodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(arspareusermodels);
        }

        // GET: /ArSpareUser/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareUserModels arspareusermodels = await db.ArSpareUsers.FindAsync(id);
            if (arspareusermodels == null)
            {
                return HttpNotFound();
            }
            return View(arspareusermodels);
        }

        // POST: /ArSpareUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ArSpareUserModels arspareusermodels = await db.ArSpareUsers.FindAsync(id);
            db.ArSpareUsers.Remove(arspareusermodels);
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
