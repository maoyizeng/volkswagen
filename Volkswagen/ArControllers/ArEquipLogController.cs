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
    public class ArEquipLogController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArEquipLog/
        public async Task<ActionResult> Index()
        {
            return View(await db.ArEquipLogs.ToListAsync());
        }

        // GET: /ArEquipLog/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArEquipLogModels arequiplogmodels = await db.ArEquipLogs.FindAsync(id);
            if (arequiplogmodels == null)
            {
                return HttpNotFound();
            }
            return View(arequiplogmodels);
        }

        // GET: /ArEquipLog/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /ArEquipLog/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="EquipmentID,Operator,OperateTime,Department,EquipDes,Type,Spec,DocumentDate,EnableDate,OriginValue,Depreciation,Spot1,Spot2,Spot3,Remark,ChangeTime,Changer,CreateTime,Creator")] ArEquipLogModels arequiplogmodels)
        {
            if (ModelState.IsValid)
            {
                db.ArEquipLogs.Add(arequiplogmodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(arequiplogmodels);
        }

        // GET: /ArEquipLog/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArEquipLogModels arequiplogmodels = await db.ArEquipLogs.FindAsync(id);
            if (arequiplogmodels == null)
            {
                return HttpNotFound();
            }
            return View(arequiplogmodels);
        }

        // POST: /ArEquipLog/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="EquipmentID,Operator,OperateTime,Department,EquipDes,Type,Spec,DocumentDate,EnableDate,OriginValue,Depreciation,Spot1,Spot2,Spot3,Remark,ChangeTime,Changer,CreateTime,Creator")] ArEquipLogModels arequiplogmodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(arequiplogmodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(arequiplogmodels);
        }

        // GET: /ArEquipLog/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArEquipLogModels arequiplogmodels = await db.ArEquipLogs.FindAsync(id);
            if (arequiplogmodels == null)
            {
                return HttpNotFound();
            }
            return View(arequiplogmodels);
        }

        // POST: /ArEquipLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            ArEquipLogModels arequiplogmodels = await db.ArEquipLogs.FindAsync(id);
            db.ArEquipLogs.Remove(arequiplogmodels);
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
