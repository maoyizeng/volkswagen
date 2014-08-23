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
    public class ArUserController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArUser/
        public async Task<ActionResult> Index()
        {
            return View(await db.ArUsers.ToListAsync());
        }

        // GET: /ArUser/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArUserModels arusermodels = await db.ArUsers.FindAsync(id);
            if (arusermodels == null)
            {
                return HttpNotFound();
            }
            return View(arusermodels);
        }

        // GET: /ArUser/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /ArUser/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="UserID,Operator,OperateTime,Breviary,Name,Number,Telephone,Mobile,Birthday,EntryDate,Position,PoliticalStatus,Address,Skill,Experience,Remark,Image,ChangeTime,Changer,CreateTime,Creator")] ArUserModels arusermodels)
        {
            if (ModelState.IsValid)
            {
                db.ArUsers.Add(arusermodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(arusermodels);
        }

        // GET: /ArUser/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArUserModels arusermodels = await db.ArUsers.FindAsync(id);
            if (arusermodels == null)
            {
                return HttpNotFound();
            }
            return View(arusermodels);
        }

        // POST: /ArUser/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="UserID,Operator,OperateTime,Breviary,Name,Number,Telephone,Mobile,Birthday,EntryDate,Position,PoliticalStatus,Address,Skill,Experience,Remark,Image,ChangeTime,Changer,CreateTime,Creator")] ArUserModels arusermodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(arusermodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(arusermodels);
        }

        // GET: /ArUser/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArUserModels arusermodels = await db.ArUsers.FindAsync(id);
            if (arusermodels == null)
            {
                return HttpNotFound();
            }
            return View(arusermodels);
        }

        // POST: /ArUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ArUserModels arusermodels = await db.ArUsers.FindAsync(id);
            db.ArUsers.Remove(arusermodels);
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
