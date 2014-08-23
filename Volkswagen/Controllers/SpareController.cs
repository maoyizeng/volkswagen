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
using System.Text;

namespace Volkswagen.Controllers
{
    public class SpareController : Controller
    {
        private SVWContext db = new SVWContext();

        // GET: /Spare/
        /*public async Task<ActionResult> Index(GridSortOptions model)
        {
            ViewData["model"] = model;

            IQueryable<SpareModels> list = db.Spares.Where("1 = 1");
            if (!string.IsNullOrEmpty(model.Column))
            {
                list = list.OrderBy(model.Column);
            }
            else
            {
                return View(await db.Spares.ToListAsync());
            }
            return View(list);
        }*/

        // GET: /Spare/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareModels sparemodels = await db.Spares.FindAsync(id);
            if (sparemodels == null)
            {
                return HttpNotFound();
            }
            return View(sparemodels);
        }

        // GET: /Spare/
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            IQueryable<SpareModels> list = db.Spares.Where("1 = 1");
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
                return View(await db.Spares.ToListAsync());
            }
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> Index()
        {
            ParameterExpression param = Expression.Parameter(typeof(SpareModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++)
            {
                string field = Request.Form["field" + n];
                string op = Request.Form["op" + n];
                string operand = Request.Form["operand" + n];
                if (string.IsNullOrEmpty(field)) break;

                Expression left = Expression.Property(param, typeof(SpareModels).GetProperty(field));
                Expression right = Expression.Constant(operand);
                Expression result;

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
                    default:
                        result = Expression.Equal(left, right);
                        break;
                }
                filter = Expression.And(filter, result);
            }

            Expression pred = Expression.Lambda(filter, param);

            var e = db.Spares;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(SpareModels) }, Expression.Constant(e), pred);

            IQueryable<SpareModels> list = db.Spares.AsQueryable().Provider.CreateQuery<SpareModels>(expr);


            return View(list);
        }

        // GET: /Spare/Create
        public ActionResult Create()
        {
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipDes");
            return View();
        }

        // POST: /Spare/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="SpareID,SpareDes,Type,Picture1,Picture2,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,File,ChangeTime,Changer,CreateTime,Creator")] SpareModels sparemodels)
        {
            if (ModelState.IsValid)
            {
                sparemodels.Changer = User.Identity.Name;
                sparemodels.Creator = User.Identity.Name;
                sparemodels.CreateTime = DateTime.Now;
                sparemodels.ChangeTime = DateTime.Now;
                ArSpareModels ar = new ArSpareModels(sparemodels);
                ar.Operator = "Create";

                db.ArSpares.Add(ar);
                db.Spares.Add(sparemodels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", sparemodels.EquipmentID);
            return View(sparemodels);
        }

        // GET: /Spare/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareModels sparemodels = await db.Spares.FindAsync(id);
            if (sparemodels == null)
            {
                return HttpNotFound();
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", sparemodels.EquipmentID);
            return View(sparemodels);
        }

        // POST: /Spare/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="SpareID,SpareDes,Type,Picture1,Picture2,Mark,PresentValue,SafeValue,DCMinValue,DCMaxValue,Property,EquipmentID,Producer,OrderNumber,Remark,KeyPart,File,ChangeTime,Changer,CreateTime,Creator")] SpareModels sparemodels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sparemodels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.EquipmentID = new SelectList(db.Equipments, "EquipmentID", "EquipmentID", sparemodels.EquipmentID);
            return View(sparemodels);
        }

        // GET: /Spare/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SpareModels sparemodels = await db.Spares.FindAsync(id);
            if (sparemodels == null)
            {
                return HttpNotFound();
            }
            return View(sparemodels);
        }

        // POST: /Spare/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            SpareModels sparemodels = await db.Spares.FindAsync(id);
            db.Spares.Remove(sparemodels);
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

        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<SpareModels> list = db.Spares.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "设备物流编号",
        "备件名称",
        "备件型号",
        "仓位号",
        "当前库存",
        "安全库存",
        "东昌最小库存",
        "东昌最大库存",
        "所属设备",
        "设备编号",
        "设备供货商",
        "订货号",
        "设备关键属性",
        "最后修改时间",
        "修改人",
        "创建时间",
        "创建人",
        "备注"
            };
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Picture1);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Picture2);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Mark);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.PresentValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.SafeValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.DCMinValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.DCMaxValue);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Property);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.EquipmentID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Producer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.OrderNumber);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.KeyPart);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ChangeTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Changer);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.CreateTime);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Creator);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Remark);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());

            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "备件库存.xls");
        }
    }
}
