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

namespace Volkswagen.Controllers
{
    public class EquipmentController : Controller
    {
        private SVWContext db = new SVWContext();
        /*private enum operation
        {
            EQ,     // == equal to
            GT,     // >  greater than
            LT,     // <  less than
            GE,     // >= greater than or equal to
            LE,     // <= less than or equal to
            CONTAIN // contain
        };
        string[] operation = new string[] {
            "=",
            ">",
            "<",
            ">=",
            "<="
        };*/

        // GET: /Equipment/
        public async Task<ActionResult> Index()
        {
            return View(await db.Equipments.ToListAsync());
        }

        // GET: /Equipment/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipmentModels equipmentmodels = await db.Equipments.FindAsync(id);
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // GET: /Equipment/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Equipment/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="EquipmentID,EquipDes,Person,Section,WSArea,Photo,ItemInspect,RegularCare,Check,RoutingInspect,TPMFile,Rules,TechnicFile,TrainingFile,ChangeTime,Changer,CreateTime,Creator,Remark")] EquipmentModels equipmentmodels)
        {
            if (ModelState.IsValid)
            {
                db.Equipments.Add(equipmentmodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(equipmentmodels);
        }

        // GET: /Equipment/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipmentModels equipmentmodels = await db.Equipments.FindAsync(id);
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // POST: /Equipment/Query
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Query()
        {
            //var equipmentList = await db.Equipments.ToListAsync();// = await db.Equipments.Where(p => p);
            //string sql = "SELECT * FROM Equipments WHERE 1=1";

            /*for (int n = 0; ; n++) {
                string field = Request.Form["field" + n];
                string op = Request.Form["op" + n];
                string operand = Request.Form["operand" + n];
                if (string.IsNullOrEmpty(field)) break;
                switch (Convert.ToByte(op))
                {
                    case 0:
                        sql += " AND p." + field + "=" + operand;
                        break;
                    case 1:
                        sql += " AND p." + field + ">" + operand;
                        break;
                    case 2:
                        sql += " AND p." + field + "<" + operand;
                        break;
                    case 3:
                        sql += " AND p." + field + ">=" + operand;
                        break;
                    case 4:
                        sql += " AND p." + field + "<=" + operand;
                        break;
                    case 5:
                        //sql += " AND " + field + ">" + operand;
                        break;
                    default:
                        break;
                }
            }*/

            ParameterExpression param = Expression.Parameter(typeof(EquipmentModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++) {
                string field = Request.Form["field" + n];
                string op = Request.Form["op" + n];
                string operand = Request.Form["operand" + n];
                if (string.IsNullOrEmpty(field)) break;

                Expression left = Expression.Property(param, typeof(EquipmentModels).GetProperty(field));
                Expression right = Expression.Constant(operand);
                Expression result;

                switch (Convert.ToByte(op))
                {
                    case 0:
                        result = Expression.Equal(left, right);
                        break;
                    case 1:
                        result = Expression.GreaterThan(left, right);
                        break;
                    case 2:
                        result = Expression.LessThan(left, right);
                        break;
                    case 3:
                        result = Expression.GreaterThanOrEqual(left, right);
                        break;
                    case 4:
                        result = Expression.LessThanOrEqual(left, right);
                        break;
                    case 5:
                        result = Expression.Equal(left, right);
                        break;
                    default:
                        result = Expression.Equal(left, right);
                        break;
                }
                filter = Expression.And(filter, result);
            }

            Expression pred = Expression.Lambda(filter, param);

            var e = db.Equipments;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(EquipmentModels) }, Expression.Constant(e), pred);

            //IQueryable<EquipmentModels> equipmentList = db.Equipments.AsQueryable().Provider.CreateQuery<EquipmentModels>(expr).ToListAsync();
           // var equipmentList = db.Equipments.AsQueryable().Provider.CreateQuery<EquipmentModels>(expr).ToListAsync();
            return View(db.Equipments.AsQueryable().Provider.CreateQuery<EquipmentModels>(expr).ToList());
        }

        // POST: /Equipment/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="EquipmentID,EquipDes,Person,Section,WSArea,Photo,ItemInspect,RegularCare,Check,RoutingInspect,TPMFile,Rules,TechnicFile,TrainingFile,ChangeTime,Changer,CreateTime,Creator,Remark")] EquipmentModels equipmentmodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(equipmentmodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(equipmentmodels);
        }

        // GET: /Equipment/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipmentModels equipmentmodels = await db.Equipments.FindAsync(id);
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // POST: /Equipment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            EquipmentModels equipmentmodels = await db.Equipments.FindAsync(id);
            db.Equipments.Remove(equipmentmodels);
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
