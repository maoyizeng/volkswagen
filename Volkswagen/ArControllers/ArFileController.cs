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
    public class ArFileController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArFile/
        public async Task<ActionResult> Index()
        {
            return View(await db.ArFiles.ToListAsync());
        }

        // GET: /ArFile/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArFileModels arfilemodels = await db.ArFiles.FindAsync(id);
            if (arfilemodels == null)
            {
                return HttpNotFound();
            }
            return View(arfilemodels);
        }

        // GET: /ArFile/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /ArFile/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="FileName,Operator,OperateTime,Class,EquipmentID,EquipDes,Charger,File,ChangeTime,Changer,CreateTime,Creator")] ArFileModels arfilemodels)
        {
            if (ModelState.IsValid)
            {
                db.ArFiles.Add(arfilemodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(arfilemodels);
        }

        // GET: /ArFile/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArFileModels arfilemodels = await db.ArFiles.FindAsync(id);
            if (arfilemodels == null)
            {
                return HttpNotFound();
            }
            return View(arfilemodels);
        }

        // POST: /ArFile/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="FileName,Operator,OperateTime,Class,EquipmentID,EquipDes,Charger,File,ChangeTime,Changer,CreateTime,Creator")] ArFileModels arfilemodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(arfilemodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(arfilemodels);
        }

        // GET: /ArFile/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArFileModels arfilemodels = await db.ArFiles.FindAsync(id);
            if (arfilemodels == null)
            {
                return HttpNotFound();
            }
            return View(arfilemodels);
        }

        // POST: /ArFile/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            ArFileModels arfilemodels = await db.ArFiles.FindAsync(id);
            db.ArFiles.Remove(arfilemodels);
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
