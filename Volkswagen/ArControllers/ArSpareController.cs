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
    public class ArSpareController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArSpare/
        public async Task<ActionResult> Index()
        {
            return View(await db.ArSpares.ToListAsync());
        }

        // GET: /ArSpare/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareModels arsparemodels = await db.ArSpares.FindAsync(id);
            if (arsparemodels == null)
            {
                return HttpNotFound();
            }
            return View(arsparemodels);
        }

        // GET: /ArSpare/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /ArSpare/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="SpareID,Operator,OperateTime,SpareDes,Type,Picture1,Picture2,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,ChangeTime,Changer,CreateTime,Creator")] ArSpareModels arsparemodels)
        {
            if (ModelState.IsValid)
            {
                db.ArSpares.Add(arsparemodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(arsparemodels);
        }

        // GET: /ArSpare/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareModels arsparemodels = await db.ArSpares.FindAsync(id);
            if (arsparemodels == null)
            {
                return HttpNotFound();
            }
            return View(arsparemodels);
        }

        // POST: /ArSpare/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="SpareID,Operator,OperateTime,SpareDes,Type,Picture1,Picture2,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,ChangeTime,Changer,CreateTime,Creator")] ArSpareModels arsparemodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(arsparemodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(arsparemodels);
        }

        // GET: /ArSpare/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareModels arsparemodels = await db.ArSpares.FindAsync(id);
            if (arsparemodels == null)
            {
                return HttpNotFound();
            }
            return View(arsparemodels);
        }

        // POST: /ArSpare/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            ArSpareModels arsparemodels = await db.ArSpares.FindAsync(id);
            db.ArSpares.Remove(arsparemodels);
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
