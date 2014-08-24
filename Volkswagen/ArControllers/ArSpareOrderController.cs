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
    public class ArSpareOrderController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArSpareOrder/
        public async Task<ActionResult> Index()
        {
            return View(await db.ArSpareOrders.ToListAsync());
        }

        // GET: /ArSpareOrder/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareOrderModels arspareordermodels = await db.ArSpareOrders.FindAsync(id);
            if (arspareordermodels == null)
            {
                return HttpNotFound();
            }
            return View(arspareordermodels);
        }

        // GET: /ArSpareOrder/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /ArSpareOrder/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="OrderID,Operator,OperateTime,SpareID,SpareDes,Type,OrderValue,Producer,OrderNum,Property,EquipmentID,Maker,MakerNum,Orderman,UseTime,UnitPrice,TotalPrice,Status,Mode,ChangeTime,Changer,CreateTime,Creator")] ArSpareOrderModels arspareordermodels)
        {
            if (ModelState.IsValid)
            {
                db.ArSpareOrders.Add(arspareordermodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(arspareordermodels);
        }

        // GET: /ArSpareOrder/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareOrderModels arspareordermodels = await db.ArSpareOrders.FindAsync(id);
            if (arspareordermodels == null)
            {
                return HttpNotFound();
            }
            return View(arspareordermodels);
        }

        // POST: /ArSpareOrder/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="OrderID,Operator,OperateTime,SpareID,SpareDes,Type,OrderValue,Producer,OrderNum,Property,EquipmentID,Maker,MakerNum,Orderman,UseTime,UnitPrice,TotalPrice,Status,Mode,ChangeTime,Changer,CreateTime,Creator")] ArSpareOrderModels arspareordermodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(arspareordermodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(arspareordermodels);
        }

        // GET: /ArSpareOrder/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArSpareOrderModels arspareordermodels = await db.ArSpareOrders.FindAsync(id);
            if (arspareordermodels == null)
            {
                return HttpNotFound();
            }
            return View(arspareordermodels);
        }

        // POST: /ArSpareOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ArSpareOrderModels arspareordermodels = await db.ArSpareOrders.FindAsync(id);
            db.ArSpareOrders.Remove(arspareordermodels);
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
