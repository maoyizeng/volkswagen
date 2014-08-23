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
    public class ArMaintainController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArMaintain/
        public async Task<ActionResult> Index()
        {
            return View(await db.ArMaintains.ToListAsync());
        }

        // GET: /ArMaintain/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArMaintainModels armaintainmodels = await db.ArMaintains.FindAsync(id);
            if (armaintainmodels == null)
            {
                return HttpNotFound();
            }
            return View(armaintainmodels);
        }

        // GET: /ArMaintain/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /ArMaintain/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="MaintainId,Operator,OperateTime,EquipmentID,EquipDes,Line,MType,MPart,Content,Period,MStartTime,MEndTime,ResponseClass,CheckStatus,CheckDetail,EquipStatus,EquipDetail,CheckerType,Checker,CheckTime,Problem,Mark,Grade,ProblemStatus,CheckNum,ChangeTime,Changer,CreateTime,Creator")] ArMaintainModels armaintainmodels)
        {
            if (ModelState.IsValid)
            {
                db.ArMaintains.Add(armaintainmodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(armaintainmodels);
        }

        // GET: /ArMaintain/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArMaintainModels armaintainmodels = await db.ArMaintains.FindAsync(id);
            if (armaintainmodels == null)
            {
                return HttpNotFound();
            }
            return View(armaintainmodels);
        }

        // POST: /ArMaintain/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="MaintainId,Operator,OperateTime,EquipmentID,EquipDes,Line,MType,MPart,Content,Period,MStartTime,MEndTime,ResponseClass,CheckStatus,CheckDetail,EquipStatus,EquipDetail,CheckerType,Checker,CheckTime,Problem,Mark,Grade,ProblemStatus,CheckNum,ChangeTime,Changer,CreateTime,Creator")] ArMaintainModels armaintainmodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(armaintainmodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(armaintainmodels);
        }

        // GET: /ArMaintain/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArMaintainModels armaintainmodels = await db.ArMaintains.FindAsync(id);
            if (armaintainmodels == null)
            {
                return HttpNotFound();
            }
            return View(armaintainmodels);
        }

        // POST: /ArMaintain/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ArMaintainModels armaintainmodels = await db.ArMaintains.FindAsync(id);
            db.ArMaintains.Remove(armaintainmodels);
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
