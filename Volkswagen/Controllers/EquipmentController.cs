﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
//using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Volkswagen.Models;
using Volkswagen.DAL;
using System.Linq.Expressions;
using MvcContrib.UI.Grid;

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
        private string[] fieldMap = new string[] {
             "设备编号",
             "设备名称",
             "负责人",
             "所在工段",
             "车间生产线",
             "点检",
             "日常保养",
             "巡检",
             "需更新否",
             "最后修改时间",
             "修改人",
             "创建时间",
             "创建人",
             "备注"
        };*/
        private string[] operation = new string[] {
            "=",
            ">",
            "<",
            ">=",
            "<="
        };

        // GET: /Equipment/
        public async Task<ActionResult> Index()
        {
            //PrepareSelectItems();
            return View(await db.Equipments.ToListAsync());
        }

        /*public async Task<ActionResult> Index(GridSortOptions model)
        {
            ViewData["model"] = model;
            if (model == null)
            {
                return View(await db.Equipments.ToListAsync());
            }
            IQueryable<EquipmentModels> list = ViewData.Model as IQueryable<EquipmentModels>;
            if (!string.IsNullOrEmpty(model.Column))
            {
                list = list.OrderBy(model.Column);
                //list = list.OrderBy(model.Column, model.Direction);
            }
            //list = list.AsPagination(page ?? 1, 5);
            return View(list);
        }  */

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
            ViewData.Model = db.Equipments.AsQueryable().Provider.CreateQuery<EquipmentModels>(expr).ToList();
            return RedirectToAction("Index");
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

        private void PrepareSelectItems()
        {
            List<SelectListItem> fieldlist = new List<SelectListItem> {
                new SelectListItem { Text = "设备编号", Value = "EquipmentID", Selected = true},
                new SelectListItem { Text = "设备名称", Value = "EquipDes"},
                new SelectListItem { Text = "负责人", Value = "Person"},
                new SelectListItem { Text = "所在工段", Value = "Section"},
                new SelectListItem { Text = "车间生产线", Value = "WSArea"},
                new SelectListItem { Text = "点检", Value = "ItemInspect"},
                new SelectListItem { Text = "日常保养", Value = "RegularCare"},
                new SelectListItem { Text = "巡检", Value = "Check"},
                new SelectListItem { Text = "需更新否", Value = "RoutingInspect"},
                new SelectListItem { Text = "最后修改时间", Value = "ChangeTime"},
                new SelectListItem { Text = "修改人", Value = "Changer"},
                new SelectListItem { Text = "创建时间", Value = "CreateTime"},
                new SelectListItem { Text = "创建人", Value = "Creator"},
                new SelectListItem { Text = "备注", Value = "Remark"}
            };
            List<SelectListItem> operationList = new List<SelectListItem> {
                new SelectListItem { Text = "=", Value = "0", Selected = true},
                new SelectListItem { Text = ">", Value = "1"},
                new SelectListItem { Text = "<", Value = "2"},
                new SelectListItem { Text = ">=", Value = "3"},
                new SelectListItem { Text = "<=", Value = "4"},
                new SelectListItem { Text = "包含", Value = "5"}
            };
            ViewData["fields"] = fieldlist;
            ViewData["operations"] = operationList;

            //fieldRow = 0;
        }

    }
}
