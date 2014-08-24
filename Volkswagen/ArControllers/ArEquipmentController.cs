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
    public class ArEquipmentController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /ArEquipment/
        public async Task<ActionResult> Index()
        {
            return View(await db.ArEquipments.ToListAsync());
        }

        // GET: /ArEquipment/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArEquipmentModels arequipmentmodels = await db.ArEquipments.FindAsync(id);
            if (arequipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(arequipmentmodels);
        }

        // GET: /ArEquipment/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArEquipmentModels arequipmentmodels = await db.ArEquipments.FindAsync(id);
            if (arequipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(arequipmentmodels);
        }

        // POST: /ArEquipment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            ArEquipmentModels arequipmentmodels = await db.ArEquipments.FindAsync(id);
            db.ArEquipments.Remove(arequipmentmodels);
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
