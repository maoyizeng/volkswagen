﻿using System;
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
using MvcContrib.UI.Grid;
using System.Linq.Expressions;
using System.IO;
using System.Linq.Dynamic;
using MvcContrib.Sorting;
using MvcContrib.Pagination;
using System.Text;

namespace Volkswagen.Controllers
{
    [UserAuthorized]
    public class RepairController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Repair/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<RepairModels> list = getQuery(false);
            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    list = list.OrderBy(model.Column + " desc");
                }
                else
                {
                    list = list.OrderBy(model.Column + " asc");
                }
            }
            else
            {
                // 如果Column为空, 说明没有设置排序选项, 默认按报修时间排序
                list = list.OrderBy("StartTime desc");
                GridSortOptions default_model = new GridSortOptions();
                default_model.Column = "StartTime";
                default_model.Direction = SortDirection.Descending;
                ViewData["model"] = default_model;
            }

            return View(list.ToList().AsPagination(page ?? 1, 100));
        }

        [HttpPost]
        public async Task<ActionResult> Index(int? page, string selected_item)
        {
            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<RepairModels> list = getQuery();

            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    list = list.OrderBy(model.Column + " desc");
                }
                else
                {
                    list = list.OrderBy(model.Column + " asc");
                }
            }

            return View(list.ToList().AsPagination(page ?? 1, 100));
        }

        private IQueryable<RepairModels> getQuery(bool post = true)
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(RepairModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++)
            {
                string field = (post ? Request.Form["field" + n] : Request["field" + n]);
                ViewData["field" + n] = field;
                string op = (post ? Request.Form["op" + n] : Request["op" + n]);
                ViewData["op" + n] = op;
                string operand = (post ? Request.Form["operand" + n] : Request["operand" + n]);
                ViewData["operand" + n] = operand;

                if (string.IsNullOrEmpty(field)) break;
                if (string.IsNullOrEmpty(operand)) continue;

                //p.[filedn]
                Expression left = Expression.Property(param, typeof(RepairModels).GetProperty(field));
                //[operandn]
                Expression right = Expression.Constant(operand);
                Expression result;

                switch (field)
                {
                    case "Class":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(RepairModels.ClassType), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "Line":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(EquipmentModels.WSNames), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    //case "Section":
                    //    right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(RepairModels.SectionNames), operand)));
                    //    right = Expression.Convert(right, left.Type);
                    //    break;
                    case "FaultType":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(RepairModels.FaultTypeEnum), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "Result":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(EquipmentModels.YesNo), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "ChangeTime":
                    case "CreateTime":
                    case "StartTime":
                    case "FinishTime":
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "RepairTime":
                    case "StopTime":
                    case "RepairNum":
                        right = Expression.Constant(int.Parse(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    default:
                        break;
                }

                switch (op)
                {
                    case "0":
                        result = Expression.Equal(left, right);
                        break;
                    case "1":
                        result = Expression.GreaterThan(left, right);
                        break;
                    case "2":
                        result = Expression.LessThan(left, right);
                        break;
                    case "3":
                        result = Expression.GreaterThanOrEqual(left, right);
                        break;
                    case "4":
                        result = Expression.LessThanOrEqual(left, right);
                        break;
                    case "5":
                        result = Expression.NotEqual(left, right);
                        break;
                    case "6": //Contain
                        result = Expression.Call(left, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), right);
                        break;
                    default:
                        result = Expression.Equal(left, right);
                        break;
                }
                filter = Expression.And(filter, result);
            }

            // p => p.[filedn] [opn] [operandn] && ...
            Expression pred = Expression.Lambda(filter, param);

            // where(p => p.[filedn] [opn] [operandn] && ...)
            var e = db.Repairs;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(RepairModels) }, Expression.Constant(e), pred);

            IQueryable<RepairModels> list = db.Repairs.AsQueryable().Provider.CreateQuery<RepairModels>(expr);

            return list;
        }

        private List<RepairModels> getSelected(IQueryable<RepairModels> l)
        {
            List<RepairModels> list = new List<RepairModels>();
            List<RepairModels> list_origin = l.ToList();
            foreach (RepairModels e in list_origin)
            {
                var ss = Request.Form["Checked" + e.SheetID];
                if ((!string.IsNullOrEmpty(ss)) && (ss != "false"))
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /Repair/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RepairModels repairmodels = await db.Repairs.FindAsync(id);
            if (repairmodels == null)
            {
                return HttpNotFound();
            }
            return View(repairmodels);
        }

        // GET: /Repair/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
            return View();
        }

        // POST: /Repair/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="SheetID,EquipmentID,EquipDes,StartTime,FinishTime,RepairTime,Class,Line,Section,FaultView,Repairman,Description,FaultType,Result,Problem,Checker,Remark,StopTime,File,RepairNum,ChangeTime,Changer,CreateTime,Creator")] RepairModels repairmodels)
        {
            if (db.Equipments.Find(repairmodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(repairmodels);
            }
            if (db.Repairs.Find(repairmodels.SheetID) != null)
            {
                ViewData["valid"] = "exist_key";
                return View(repairmodels);
            }
            if (ModelState.IsValid)
            {
                repairmodels.Changer = User.Identity.Name;
                repairmodels.Creator = User.Identity.Name;
                repairmodels.CreateTime = DateTime.Now;
                repairmodels.ChangeTime = DateTime.Now;
                db.Repairs.Add(repairmodels);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {

                    ArRepairModels ar = new ArRepairModels(repairmodels);
                    ar.Operator = ArEquipmentModels.OperatorType.创建;
                    db.ArRepairs.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", repairmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", repairmodels.EquipDes);
            return View(repairmodels);
        }

        // GET: /Repair/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RepairModels repairmodels = await db.Repairs.FindAsync(id);
            if (repairmodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", repairmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", repairmodels.EquipDes);
            return View(repairmodels);
        }

        // POST: /Repair/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="SheetID,EquipmentID,EquipDes,StartTime,FinishTime,RepairTime,Class,Line,Section,FaultView,Repairman,Description,FaultType,Result,Problem,Checker,Remark,StopTime,File,RepairNum,ChangeTime,Changer,CreateTime,Creator")] RepairModels repairmodels)
        {
            if (db.Equipments.Find(repairmodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(repairmodels);
            }
            if (ModelState.IsValid)
            {
                var toUpdate = db.Repairs.Find(repairmodels.SheetID);

                repairmodels.Changer = User.Identity.Name;
                repairmodels.ChangeTime = DateTime.Now;
                repairmodels.Creator = toUpdate.Creator;
                repairmodels.CreateTime = toUpdate.CreateTime;

                db.Entry(toUpdate).State = EntityState.Detached;
                db.Entry(repairmodels).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArRepairModels ar = new ArRepairModels(toUpdate);
                    ar.Operator = ArEquipmentModels.OperatorType.修改;
                    db.ArRepairs.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", repairmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", repairmodels.EquipDes);
            return View(repairmodels);
        }

        // POST: /Repair/EditMultiple/
        //[HttpPost]
        public async Task<ActionResult> EditMultiple()
        {
            IQueryable<RepairModels> l = getQuery();
            List<RepairModels> list = getSelected(l);
            if (ViewData["list"] == null) ViewData["list"] = list;
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
            return View("ChangeMultiple", new RepairModels());
        }

        // POST: /Repair/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "SheetID,EquipmentID,EquipDes,StartTime,FinishTime,RepairTime,Class,Line,Section,FaultView,Repairman,Description,FaultType,Result,Problem,Checker,Remark,StopTime,File,RepairNum,ChangeTime,Changer,CreateTime,Creator")] RepairModels repairmodels)
        {
            if ((!string.IsNullOrEmpty(repairmodels.EquipmentID)) && (db.Equipments.Find(repairmodels.EquipmentID) == null))
            {
                ViewData["valid"] = "no_foreign";
                return View(repairmodels);
            }

            bool changed = false;
            List<RepairModels> l = new List<RepairModels>();
            for (int i = 0; ; i++)
            {
                string id = Request.Form["item" + i];
                if (Request.Form["item" + i] == null) break;
                RepairModels e = db.Repairs.Find(id);
                l.Add(e);
                ArRepairModels ar = new ArRepairModels(e); 
                if (repairmodels.EquipmentID != null && ModelState.IsValidField("EquipmentID")) e.EquipmentID = repairmodels.EquipmentID;
                if (repairmodels.EquipDes != null && ModelState.IsValidField("EquipDes")) e.EquipDes = repairmodels.EquipDes;
                if (repairmodels.Class != null && ModelState.IsValidField("Class")) e.Class = repairmodels.Class;
                if (repairmodels.StartTime != null && ModelState.IsValidField("StartTime")) e.StartTime = repairmodels.StartTime;
                if (repairmodels.FinishTime != null && ModelState.IsValidField("FinishTime")) e.FinishTime = repairmodels.FinishTime;
                if (repairmodels.RepairTime != null && ModelState.IsValidField("RepairTime")) e.RepairTime = repairmodels.RepairTime;
                if (repairmodels.Line != null && ModelState.IsValidField("Line")) e.Line = repairmodels.Line;
                if (repairmodels.Section != null && ModelState.IsValidField("Section")) e.Section = repairmodels.Section;
                if (repairmodels.FaultView != null && ModelState.IsValidField("FaultView")) e.FaultView = repairmodels.FaultView;
                if (repairmodels.Repairman != null && ModelState.IsValidField("Repairman")) e.Repairman = repairmodels.Repairman;
                if (repairmodels.Description != null && ModelState.IsValidField("Description")) e.Description = repairmodels.Description;
                if (repairmodels.FaultType != null && ModelState.IsValidField("FaultType")) e.FaultType = repairmodels.FaultType;
                if (repairmodels.EquipDes != null && ModelState.IsValidField("EquipDes")) e.EquipDes = repairmodels.EquipDes;
                if (repairmodels.Result != null && ModelState.IsValidField("Result")) e.Result = repairmodels.Result;
                if (repairmodels.Problem != null && ModelState.IsValidField("Problem")) e.Problem = repairmodels.Problem;
                if (repairmodels.Checker != null && ModelState.IsValidField("Checker")) e.Checker = repairmodels.Checker;
                if (repairmodels.Remark != null && ModelState.IsValidField("Remark")) e.Remark = repairmodels.Remark;
                if (repairmodels.StopTime != null && ModelState.IsValidField("StopTime")) e.StopTime = repairmodels.StopTime;
                if (repairmodels.RepairNum != null && ModelState.IsValidField("RepairNum")) e.RepairNum = repairmodels.RepairNum;

                if (db.Entry(e).State == EntityState.Modified)
                {
                    e.Changer = User.Identity.Name;
                    e.ChangeTime = DateTime.Now;
                    int x = await db.SaveChangesAsync();
                    if (x != 0)
                    {
                        changed = true;
                        ar.Operator = ArEquipmentModels.OperatorType.修改;
                        db.ArRepairs.Add(ar);
                        await db.SaveChangesAsync();
                    }
                }
            }
            if (changed)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewData["list"] = l;
                ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
                ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
                return View(new RepairModels());
            }
        }


        // GET: /Repair/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RepairModels repairmodels = await db.Repairs.FindAsync(id);
            if (repairmodels == null)
            {
                return HttpNotFound();
            }
            return View(repairmodels);
        }

        // POST: /Repair/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            RepairModels toDelete = await db.Repairs.FindAsync(id);
            ArRepairModels ar = new ArRepairModels(toDelete);
            db.Repairs.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.删除;
                db.ArRepairs.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /Repair/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple(int? page, string selected_item)
        {
            IQueryable<RepairModels> l = getQuery();
            List<RepairModels> list = getSelected(l);
            foreach (RepairModels e in list)
            {
                ArRepairModels ar = new ArRepairModels(e);
                db.Repairs.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ar.Operator = ArEquipmentModels.OperatorType.删除;
                    db.ArRepairs.Add(ar);
                    await db.SaveChangesAsync();
                }
            }

            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            l = getQuery();

            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    l = l.OrderBy(model.Column + " desc");
                }
                else
                {
                    l = l.OrderBy(model.Column + " asc");
                }
            }

            return View("Index", l.ToList().AsPagination(page ?? 1, 100));//RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: /Repair/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;

            var l = getQuery();

            // 排序
            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    l = l.OrderBy(model.Column + " desc");
                }
                else
                {
                    l = l.OrderBy(model.Column + " asc");
                }
            }

            var list = l.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
        "报修单号",
        "设备名称",
        "设备编号",
        "报修时刻",
        "修复时刻",
        "维修耗时",
        "班次",
        "车间生产线",
        "工段",
        "故障现象",
        "维修人",
        "故障原因和维修内容",
        "故障类别",
        "已修复否",
        "遗留问题",
        "验收人",
        "备注",
        "停机时间",
        "维修次数",
        "最后修改时间",
        "修改人",
        "创建时间",
        "创建人" };
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            foreach (var i in list)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.SheetID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipDes);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.StartTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.FinishTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.RepairTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Class);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Line);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Section);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.FaultView);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Repairman);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Description);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.FaultType);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Result);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Problem);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Checker);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Remark);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.StopTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.RepairNum);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ChangeTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Changer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CreateTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Creator);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "设备报修单.xls");
        }
    }
}
