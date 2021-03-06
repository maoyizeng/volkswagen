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
    public class MaintainController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Maintain/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<MaintainModels> list = getQuery(false);
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

        [HttpPost]
        public async Task<ActionResult> Index(int? page, string selected_item)
        {
            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<MaintainModels> list = getQuery();

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

        private IQueryable<MaintainModels> getQuery(bool post = true)
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(MaintainModels), "p");
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
                Expression left = Expression.Property(param, typeof(MaintainModels).GetProperty(field));
                //[operandn]
                Expression right = Expression.Constant(operand);
                Expression result;

                switch (field)
                {
                    case "ResponseClass":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(RepairModels.ClassType), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "Line":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(EquipmentModels.WSNames), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "ChangeTime":
                    case "CreateTime":
                    case "MStartTime":
                    case "MEndTime":
                    case "CheckTime":
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "CheckNum":
                    case "MaintainId":
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
            var e = db.Maintains;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(MaintainModels) }, Expression.Constant(e), pred);

            IQueryable<MaintainModels> list = db.Maintains.AsQueryable().Provider.CreateQuery<MaintainModels>(expr);

            return list;
        }

        private List<MaintainModels> getSelected(IQueryable<MaintainModels> l)
        {
            List<MaintainModels> list = new List<MaintainModels>();
            List<MaintainModels> list_origin = l.ToList();
            foreach (MaintainModels e in list_origin)
            {
                var ss = Request.Form["Checked" + e.MaintainId];
                if ((!string.IsNullOrEmpty(ss)) && (ss != "false"))
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /Maintain/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaintainModels maintainmodels = await db.Maintains.FindAsync(id);
            if (maintainmodels == null)
            {
                return HttpNotFound();
            }
            return View(maintainmodels);
        }

        // GET: /Maintain/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
            return View();
        }

        // POST: /Maintain/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="EquipmentID,EquipDes,Line,MType,MPart,Content,Period,MStartTime,MEndTime,ResponseClass,CheckStatus,CheckDetail,EquipStatus,EquipDetail,CheckerType,Checker,CheckTime,Problem,Mark,Grade,ProblemStatus,CheckNum,ChangeTime,Changer,CreateTime,Creator")] MaintainModels maintainmodels)
        {
            if (db.Equipments.Find(maintainmodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(maintainmodels);
            }
            if (ModelState.IsValid)
            {
                maintainmodels.Changer = User.Identity.Name;
                maintainmodels.Creator = User.Identity.Name;
                maintainmodels.CreateTime = DateTime.Now;
                maintainmodels.ChangeTime = DateTime.Now;
                db.Maintains.Add(maintainmodels);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    var new_ins = db.Maintains.OrderByDescending(p => p.MaintainId).First();
                    ArMaintainModels ar = new ArMaintainModels(new_ins);
                    ar.Operator = ArEquipmentModels.OperatorType.创建;
                    db.ArMaintains.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", maintainmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", maintainmodels.EquipDes);
            return View(maintainmodels);
        }

        // GET: /Maintain/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaintainModels maintainmodels = await db.Maintains.FindAsync(id);
            if (maintainmodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", maintainmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", maintainmodels.EquipDes);
            return View(maintainmodels);
        }

        // POST: /Maintain/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="MaintainId,EquipmentID,EquipDes,Line,MType,MPart,Content,Period,MStartTime,MEndTime,ResponseClass,CheckStatus,CheckDetail,EquipStatus,EquipDetail,CheckerType,Checker,CheckTime,Problem,Mark,Grade,ProblemStatus,CheckNum,ChangeTime,Changer,CreateTime,Creator")] MaintainModels maintainmodels)
        {
            if (db.Equipments.Find(maintainmodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(maintainmodels);
            }
            if (ModelState.IsValid)
            {
                var toUpdate = db.Maintains.Find(maintainmodels.MaintainId);

                maintainmodels.Changer = User.Identity.Name;
                maintainmodels.ChangeTime = DateTime.Now;
                maintainmodels.Creator = toUpdate.Creator;
                maintainmodels.CreateTime = toUpdate.CreateTime;

                db.Entry(toUpdate).State = EntityState.Detached;
                db.Entry(maintainmodels).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArMaintainModels ar = new ArMaintainModels(toUpdate);
                    ar.Operator = ArEquipmentModels.OperatorType.修改;
                    db.ArMaintains.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", maintainmodels.EquipmentID);
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes", maintainmodels.EquipDes);
            return View(maintainmodels);
        }

        // POST: /Maintain/EditMultiple/
        //[HttpPost]
        public async Task<ActionResult> EditMultiple()
        {
            IQueryable<MaintainModels> l = getQuery();
            List<MaintainModels> list = getSelected(l);
            if (ViewData["list"] == null) ViewData["list"] = list;
            //string key = list.First().MaintainID;
            //return RedirectToAction("Edit", new { id = key });
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.EquipDes = new SelectList(db.Equipments, "EquipDes", "EquipDes");
            return View("ChangeMultiple", new MaintainModels());
        }

        // POST: /Maintain/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "MaintainId,EquipmentID,EquipDes,Line,MType,MPart,Content,Period,MStartTime,MEndTime,ResponseClass,CheckStatus,CheckDetail,EquipStatus,EquipDetail,CheckerType,Checker,CheckTime,Problem,Mark,Grade,ProblemStatus,CheckNum,ChangeTime,Changer,CreateTime,Creator")] MaintainModels maintainmodels)
        {
            if ((!string.IsNullOrEmpty(maintainmodels.EquipmentID)) && (db.Equipments.Find(maintainmodels.EquipmentID) == null))
            {
                ViewData["valid"] = "no_foreign";
                return View(maintainmodels);
            }

            bool changed = false;
            List<MaintainModels> l = new List<MaintainModels>();
            for (int i = 0; ; i++)
            {
                if (Request.Form["item" + i] == null) break;
                int id = int.Parse(Request.Form["item" + i]); 
                MaintainModels e = db.Maintains.Find(id);
                l.Add(e);
                ArMaintainModels ar = new ArMaintainModels(e);
                if (maintainmodels.EquipmentID != null && ModelState.IsValidField("EquipmentID")) e.EquipmentID = maintainmodels.EquipmentID;
                if (maintainmodels.EquipDes != null && ModelState.IsValidField("EquipDes")) e.EquipDes = maintainmodels.EquipDes;
                if (maintainmodels.Line != null && ModelState.IsValidField("Line")) e.Line = maintainmodels.Line;
                if (maintainmodels.MType != null && ModelState.IsValidField("MType")) e.MType = maintainmodels.MType;
                if (maintainmodels.MPart != null && ModelState.IsValidField("MPart")) e.MPart = maintainmodels.MPart;
                if (maintainmodels.MPart != null && ModelState.IsValidField("MPart")) e.MPart = maintainmodels.MPart;
                if (maintainmodels.Content != null && ModelState.IsValidField("Content")) e.Content = maintainmodels.Content;
                if (maintainmodels.Period != null && ModelState.IsValidField("Period")) e.Period = maintainmodels.Period;
                if (maintainmodels.MStartTime != null && ModelState.IsValidField("MStartTime")) e.MStartTime = maintainmodels.MStartTime;
                if (maintainmodels.MEndTime != null && ModelState.IsValidField("MEndTime")) e.MEndTime = maintainmodels.MEndTime;
                if (maintainmodels.ResponseClass != null && ModelState.IsValidField("ResponseClass")) e.ResponseClass = maintainmodels.ResponseClass;
                if (maintainmodels.CheckStatus != null && ModelState.IsValidField("CheckStatus")) e.CheckStatus = maintainmodels.CheckStatus;
                if (maintainmodels.EquipDetail != null && ModelState.IsValidField("EquipDetail")) e.EquipDetail = maintainmodels.EquipDetail;
                if (maintainmodels.CheckerType != null && ModelState.IsValidField("CheckerType")) e.CheckerType = maintainmodels.CheckerType;
                if (maintainmodels.Checker != null && ModelState.IsValidField("Checker")) e.Checker = maintainmodels.Checker;
                if (maintainmodels.CheckTime != null && ModelState.IsValidField("CheckTime")) e.CheckTime = maintainmodels.CheckTime;
                if (maintainmodels.Problem != null && ModelState.IsValidField("Problem")) e.Problem = maintainmodels.Problem;
                if (maintainmodels.Mark != null && ModelState.IsValidField("Mark")) e.Mark = maintainmodels.Mark;
                if (maintainmodels.Grade != null && ModelState.IsValidField("Grade")) e.Grade = maintainmodels.Grade;
                if (maintainmodels.ProblemStatus != null && ModelState.IsValidField("ProblemStatus")) e.ProblemStatus = maintainmodels.ProblemStatus;
                if (maintainmodels.CheckNum != null && ModelState.IsValidField("CheckNum")) e.CheckNum = maintainmodels.CheckNum;

                if (db.Entry(e).State == EntityState.Modified)
                {
                    e.Changer = User.Identity.Name;
                    e.ChangeTime = DateTime.Now;
                    int x = await db.SaveChangesAsync();
                    if (x != 0)
                    {
                        changed = true;
                        ar.Operator = ArEquipmentModels.OperatorType.修改;
                        db.ArMaintains.Add(ar);
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
                return View(new MaintainModels());
            }
        }


        // GET: /Maintain/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaintainModels maintainmodels = await db.Maintains.FindAsync(id);
            if (maintainmodels == null)
            {
                return HttpNotFound();
            }
            return View(maintainmodels);
        }

        // POST: /Maintain/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            MaintainModels toDelete = await db.Maintains.FindAsync(id);
            ArMaintainModels ar = new ArMaintainModels(toDelete);
            db.Maintains.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.删除;
                db.ArMaintains.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /Maintain/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple(int? page, string selected_item)
        {
            IQueryable<MaintainModels> l = getQuery();
            List<MaintainModels> list = getSelected(l);
            foreach (MaintainModels e in list)
            {
                ArMaintainModels ar = new ArMaintainModels(e);
                db.Maintains.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ar.Operator = ArEquipmentModels.OperatorType.删除;
                    db.ArMaintains.Add(ar);
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
            return View("Index", l.ToList().AsPagination(page ?? 1, 100));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: /Maintain/ExportExcel
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
                "设备编号",
                "设备名称",
                "维护类别",
                "保养部件",
                "保养位置",
                "保养内容",
                "保养周期",
                "注意事项",
                "备注",
                "编号",
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipDes);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Line);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MType);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MPart);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Content);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Period);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MStartTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MEndTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ResponseClass);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckStatus);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipDetail);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckerType);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Checker);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Problem);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Mark);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Grade);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ProblemStatus);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CheckNum);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MaintainId);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ChangeTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Changer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CreateTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Creator);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "设备保养计划.xls");
        }
    }
}
