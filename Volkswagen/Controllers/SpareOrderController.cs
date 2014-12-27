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
using MvcContrib.UI.Grid;
using System.Linq.Expressions;
using System.IO;
using System.Linq.Dynamic;
using MvcContrib.Sorting;
using MvcContrib.Pagination;
using System.Text;

namespace Volkswagen.Controllers
{
    public class SpareOrderController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /SpareOrder/
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            IQueryable<SpareOrderModels> list = getQuery(false);
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

            IQueryable<SpareOrderModels> list = getQuery();

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

        private IQueryable<SpareOrderModels> getQuery(bool post = true)
        {
            //p
            ParameterExpression param = Expression.Parameter(typeof(SpareOrderModels), "p");
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
                Expression left = Expression.Property(param, typeof(SpareOrderModels).GetProperty(field));
                //[operandn]
                Expression right = Expression.Constant(operand);
                Expression result;

                switch (field)
                {
                    case "ChangeTime":
                    case "CreateTime":
                    case "UseTime":
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "OrderValue":
                    case "UnitPrice":
                    case "TotalPrice":
                    case "OrderID":
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
            var e = db.SpareOrders;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(SpareOrderModels) }, Expression.Constant(e), pred);

            IQueryable<SpareOrderModels> list = db.SpareOrders.AsQueryable().Provider.CreateQuery<SpareOrderModels>(expr);

            return list;
        }

        private List<SpareOrderModels> getSelected(IQueryable<SpareOrderModels> l)
        {
            List<SpareOrderModels> list = new List<SpareOrderModels>();
            List<SpareOrderModels> list_origin = l.ToList();
            foreach (SpareOrderModels e in list_origin)
            {
                var ss = Request.Form["Checked" + e.OrderID];
                if ((!string.IsNullOrEmpty(ss)) && (ss != "false"))
                {
                    list.Add(e);
                }
            }

            return list;
        }

        // GET: /SpareOrder/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareOrderModels spareordermodels = await db.SpareOrders.FindAsync(id);
            if (spareordermodels == null)
            {
                return HttpNotFound();
            }
            return View(spareordermodels);
        }

        // GET: /SpareOrder/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID");
            ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes");
            return View();
        }

        // POST: /SpareOrder/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="SpareID,SpareDes,Type,OrderValue,Producer,OrderNum,Property,EquipmentID,Maker,MakerNum,Orderman,UseTime,UnitPrice,TotalPrice,Status,Mode,OrderFile,ChangeTime,Changer,CreateTime,Creator")] SpareOrderModels spareordermodels)
        {
            if (db.Spares.Find(spareordermodels.SpareID) == null)
            {
                ViewData["valid"] = "no_spare";
                return View(spareordermodels);
            }
            if (db.Equipments.Find(spareordermodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(spareordermodels);
            }

            if (ModelState.IsValid)
            {
                spareordermodels.Changer = User.Identity.Name;
                spareordermodels.Creator = User.Identity.Name;
                spareordermodels.CreateTime = DateTime.Now;
                spareordermodels.ChangeTime = DateTime.Now;
                db.SpareOrders.Add(spareordermodels);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    var new_ins = db.SpareOrders.OrderByDescending(p => p.OrderID).First();
                    ArSpareOrderModels ar = new ArSpareOrderModels(new_ins);
                    ar.Operator = ArEquipmentModels.OperatorType.创建;
                    db.ArSpareOrders.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", spareordermodels.EquipmentID);
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID", spareordermodels.SpareID);
            ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes", spareordermodels.SpareDes);
            return View(spareordermodels);
        }

        // GET: /SpareOrder/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareOrderModels spareordermodels = await db.SpareOrders.FindAsync(id);
            if (spareordermodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", spareordermodels.EquipmentID);
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID", spareordermodels.SpareID);
            ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes", spareordermodels.SpareDes); 
            return View(spareordermodels);
        }

        // POST: /SpareOrder/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="OrderID,SpareID,SpareDes,Type,OrderValue,Producer,OrderNum,Property,EquipmentID,Maker,MakerNum,Orderman,UseTime,UnitPrice,TotalPrice,Status,Mode,OrderFile,ChangeTime,Changer,CreateTime,Creator")] SpareOrderModels spareordermodels)
        {
            if (db.Spares.Find(spareordermodels.SpareID) == null)
            {
                ViewData["valid"] = "no_spare";
                return View(spareordermodels);
            }
            if (db.Equipments.Find(spareordermodels.EquipmentID) == null)
            {
                ViewData["valid"] = "no_foreign";
                return View(spareordermodels);
            }
            if (ModelState.IsValid)
            {
                var toUpdate = db.SpareOrders.Find(spareordermodels.OrderID);

                spareordermodels.Changer = User.Identity.Name;
                spareordermodels.ChangeTime = DateTime.Now;
                spareordermodels.Creator = toUpdate.Creator;
                spareordermodels.CreateTime = toUpdate.CreateTime;

                db.Entry(toUpdate).State = EntityState.Detached;
                db.Entry(spareordermodels).State = EntityState.Modified;

                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArSpareOrderModels ar = new ArSpareOrderModels(toUpdate);
                    ar.Operator = ArEquipmentModels.OperatorType.修改;
                    db.ArSpareOrders.Add(ar);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", spareordermodels.EquipmentID);
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID", spareordermodels.SpareID);
            ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes", spareordermodels.SpareDes);
            return View(spareordermodels);
        }

        // POST: /SpareOrder/EditMultiple/
        //[HttpPost]
        public async Task<ActionResult> EditMultiple()
        {
            IQueryable<SpareOrderModels> l = getQuery();
            List<SpareOrderModels> list = getSelected(l);
            if (ViewData["list"] == null) ViewData["list"] = list;
            //string key = list.First().SpareOrderID;
            //return RedirectToAction("Edit", new { id = key });
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID");
            ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID");
            ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes");
            return View("ChangeMultiple", new SpareOrderModels());
        }

        // POST: /SpareOrder/ChangeMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "OrderID,SpareID,SpareDes,Type,OrderValue,Producer,OrderNum,Property,EquipmentID,Maker,MakerNum,Orderman,UseTime,UnitPrice,TotalPrice,Status,Mode,OrderFile,ChangeTime,Changer,CreateTime,Creator")] SpareOrderModels spareordermodels)
        {
            if ((!string.IsNullOrEmpty(spareordermodels.SpareID)) && (db.Spares.Find(spareordermodels.SpareID) == null))
            {
                ViewData["valid"] = "no_spare";
                return View(spareordermodels);
            }
            if ((!string.IsNullOrEmpty(spareordermodels.EquipmentID)) && (db.Equipments.Find(spareordermodels.EquipmentID) == null))
            {
                ViewData["valid"] = "no_foreign";
                return View(spareordermodels);
            }
            bool changed = false;
            List<SpareOrderModels> l = new List<SpareOrderModels>();
            for (int i = 0; ; i++)
            {
                if (Request.Form["item" + i] == null) break;
                int id = int.Parse(Request.Form["item" + i]);
                SpareOrderModels e = db.SpareOrders.Find(id);
                l.Add(e);
                ArSpareOrderModels ar = new ArSpareOrderModels(e);
                if (spareordermodels.EquipmentID != null && ModelState.IsValidField("EquipmentID")) e.EquipmentID = spareordermodels.EquipmentID;
                if (spareordermodels.SpareID != null && ModelState.IsValidField("SpareID")) e.SpareID = spareordermodels.SpareID;
                if (spareordermodels.SpareDes != null && ModelState.IsValidField("SpareDes")) e.SpareDes = spareordermodels.SpareDes;
                if (spareordermodels.Type != null && ModelState.IsValidField("Type")) e.Type = spareordermodels.Type;
                if (spareordermodels.OrderValue != null && ModelState.IsValidField("OrderValue")) e.OrderValue = spareordermodels.OrderValue;
                if (spareordermodels.Producer != null && ModelState.IsValidField("Producer")) e.Producer = spareordermodels.Producer;
                if (spareordermodels.OrderNum != null && ModelState.IsValidField("OrderNum")) e.OrderNum = spareordermodels.OrderNum;
                if (spareordermodels.Property != null && ModelState.IsValidField("Property")) e.Property = spareordermodels.Property;
                if (spareordermodels.Maker != null && ModelState.IsValidField("Maker")) e.Maker = spareordermodels.Maker;
                if (spareordermodels.MakerNum != null && ModelState.IsValidField("MakerNum")) e.MakerNum = spareordermodels.MakerNum;
                if (spareordermodels.Orderman != null && ModelState.IsValidField("Orderman")) e.Orderman = spareordermodels.Orderman;
                if (spareordermodels.UseTime != null && ModelState.IsValidField("UseTime")) e.UseTime = spareordermodels.UseTime;
                if (spareordermodels.UnitPrice != null && ModelState.IsValidField("UnitPrice")) e.UnitPrice = spareordermodels.UnitPrice;
                if (spareordermodels.TotalPrice != null && ModelState.IsValidField("TotalPrice")) e.TotalPrice = spareordermodels.TotalPrice;
                if (spareordermodels.Status != null && ModelState.IsValidField("Status")) e.Status = spareordermodels.Status;
                if (spareordermodels.Mode != null && ModelState.IsValidField("Mode")) e.Mode = spareordermodels.Mode;

                if (db.Entry(e).State == EntityState.Modified)
                {
                    e.Changer = User.Identity.Name;
                    e.ChangeTime = DateTime.Now;
                    int x = await db.SaveChangesAsync();
                    if (x != 0)
                    {
                        changed = true;
                        ar.Operator = ArEquipmentModels.OperatorType.修改;
                        db.ArSpareOrders.Add(ar);
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
                ViewBag.SpareID = new SelectList(db.Spares, "SpareID", "SpareID");
                ViewBag.SpareDes = new SelectList(db.Spares, "SpareDes", "SpareDes");
                return View(new SpareOrderModels());
            }
        }


        // GET: /SpareOrder/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareOrderModels spareordermodels = await db.SpareOrders.FindAsync(id);
            if (spareordermodels == null)
            {
                return HttpNotFound();
            }
            return View(spareordermodels);
        }

        // POST: /SpareOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SpareOrderModels toDelete = await db.SpareOrders.FindAsync(id);
            ArSpareOrderModels ar = new ArSpareOrderModels(toDelete);
            db.SpareOrders.Remove(toDelete);
            int x = await db.SaveChangesAsync();
            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.删除;
                db.ArSpareOrders.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /SpareOrder/DeleteMultiple/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMultiple(int? page, string selected_item)
        {
            IQueryable<SpareOrderModels> l = getQuery();
            List<SpareOrderModels> list = getSelected(l);
            foreach (SpareOrderModels e in list)
            {
                ArSpareOrderModels ar = new ArSpareOrderModels(e);
                db.SpareOrders.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ar.Operator = ArEquipmentModels.OperatorType.删除;
                    db.ArSpareOrders.Add(ar);
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

        // GET: /SpareOrder/ExportExcel
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
                "备件物流编号",
        "备件名称",
        "备件型号",
        "订购数量",
        "备件制造商",
        "订货号",
        "所属设备",
        "设备编号",
        "设备商",
        "产品编号",
        "订购人",
        "订购日期",
        "单价金额",
        "总金额",
        "状态",
        "采购形式",
        "编号",
        "最后修改时间",
        "修改人",
        "创建时间",
        "创建人"};
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            foreach (var i in list)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.SpareID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.SpareDes);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Type);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OrderValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Producer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OrderNum);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Property);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Maker);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.MakerNum);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Orderman);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.UseTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.UnitPrice);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.TotalPrice);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Status);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Mode);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OrderID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ChangeTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Changer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CreateTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Creator);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "备件订购信息.xls");
        }
    }
}
