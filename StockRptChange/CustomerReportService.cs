using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.App.Stock.Report;

namespace StockRptChange
{
    public class CustomerReportService : StockClassStaRpt
    {
        //定义临时表数组
        private string[] _customRptTempDt;

        /// <summary>
        /// 作用:1）创建自定义临时表并保存初步查询结果 2)将第1步提出的查询结果与新SQL结合并INTO至基类默认的临时表
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="tableName"></param>
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            //创建临时表,用于存放自已的数据
            var dbservice = ServiceHelper.GetService<IDBService>();
            _customRptTempDt = dbservice.CreateTemporaryTableName(Context, 1);
            var strDt = _customRptTempDt[0];

            //调用基类的方法,获取初步的查询结果到临时表
            base.BuilderReportSqlAndTempTable(filter, strDt);

            //对初步的查询结果进行处理,然后写回基类默认的存放查询结果的临时表
            //var sb=new StringBuilder();
            var strSql=$@"
                             SELECT T1.*,T2.F_YTC_DECIMAL3
                             INTO {tableName}
                             FROM {strDt} T1 /*后台'物料收发分类统计表'临时表*/
                             INNER JOIN T_BD_MATERIAL T2 ON T1.FMATERIALID=T2.FMATERIALID
                         ";
            //sb.AppendFormat(strSql, tableName, strDt);
            DBUtils.Execute(Context, strSql);//sb.ToString()
        }

        /// <summary>
        /// 重写报表动态列
        /// (注:当在BOS里新增列没有生效时使用;若重写此方法在运行后只查看到新增列而没有原来的列,就不能重写此方法,需将新列在BOS里新增)
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            var reportHeader = base.GetReportHeaders(filter);
            //添加新增列
            reportHeader.AddChild("F_YTC_DECIMAL3", new LocaleValue("包装系数", this.Context.LogLocale.LCID));
            return reportHeader;
        }

        /// <summary>
        /// 关闭报表时执行
        /// </summary>
        public override void CloseReport()
        {
            //删除临时表
            if (_customRptTempDt.IsNullOrEmptyOrWhiteSpace())
            {
                return;
            }
            var dbService = ServiceHelper.GetService<IDBService>();
            //使用后的临时表删除
            dbService.DeleteTemporaryTableName(Context, _customRptTempDt);

            base.CloseReport();
        }
    }
}
