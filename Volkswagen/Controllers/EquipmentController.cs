using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
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
        public async Task<ActionResult> Index(GridSortOptions model)
        {
            //PrepareSelectItems();
            //return View(await db.Equipments.ToListAsync());

            ViewData["model"] = model;

            IQueryable<EquipmentModels> list = db.Equipments.Where("1 = 1");
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
                return View(await db.Equipments.ToListAsync());
            }
            //list = list.AsPagination(page ?? 1, 5);
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> Index()
        {
            //IQueryable<EquipmentModels> list = ViewData.Model as IQueryable<EquipmentModels>;
            //IQueryable<EquipmentModels> list = db.Equipments.Where("1 = 1");

            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;

            IQueryable<EquipmentModels> list = getQuery();

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
            
            return View(list);
        }

        private IQueryable<EquipmentModels> getQuery()
        {
            ParameterExpression param = Expression.Parameter(typeof(EquipmentModels), "p");
            Expression filter = Expression.Constant(true);
            for (int n = 0; ; n++)
            {
                string field = Request.Form["field" + n];
                ViewData["field" + n] = field;
                string op = Request.Form["op" + n];
                ViewData["op" + n] = op;
                string operand = Request.Form["operand" + n];
                ViewData["operand" + n] = operand;

                if (string.IsNullOrEmpty(field)) break;

                Expression left = Expression.Property(param, typeof(EquipmentModels).GetProperty(field));
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

            var e = db.Equipments;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(EquipmentModels) }, Expression.Constant(e), pred);

            IQueryable<EquipmentModels> list = db.Equipments.AsQueryable().Provider.CreateQuery<EquipmentModels>(expr);

            return list;
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
            /*var equipmentList = await db.Equipments.ToListAsync();// = await db.Equipments.Where(p => p);
            string sql = "1=1";

            for (int n = 0; ; n++)
            {
                string field = Request.Form["field" + n];
                string op = Request.Form["op" + n];
                string operand = Request.Form["operand" + n];
                if (string.IsNullOrEmpty(field)) break;

                sql += " AND " + field + " " + op + " " + operand;

            //}

            ViewData.Model = db.Equipments.Where(sql);

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

            var e = db.Equipments;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(EquipmentModels) }, Expression.Constant(e), pred);

            ViewData.Model = db.Equipments.AsQueryable().Provider.CreateQuery<EquipmentModels>(expr).ToList();
            //return RedirectToAction("Index");
            return View();
        }

        // POST: /Equipment/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="EquipmentID,EquipDes,Person,Section,WSArea,Photo,ItemInspect,RegularCare,Check,RoutingInspect,ChangeTime,Changer,CreateTime,Creator,Remark")] EquipmentModels equipmentmodels)
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

        /*private void PrepareSelectItems()
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
        }*/

        // POST: /Equipment/FileUpload
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult FileUpload()
        {
            HttpPostedFileBase file = Request.Files["file"];
            if (file != null)
            {
                string filePath = Path.Combine( (AppDomain.CurrentDomain.BaseDirectory + @"uploads\"), Path.GetFileName(file.FileName));
                //file.SaveAs(Server.MapPath(@"UploadFile\" + file.FileName));
                file.SaveAs(filePath);
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Edit", new { id = 2 });
            }
            
        }

        // GET: /Equipment/ExportExcel
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            List<EquipmentModels> list = db.Equipments.ToList();

            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
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
                "备注" };
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
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Person);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Section);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.WSArea);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.ItemInspect);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.RegularCare);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.Check);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", i.RoutingInspect);
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
            return File(fileStream, "application/ms-excel", "设备履历.xls");
        }
    }
}
