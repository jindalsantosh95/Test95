using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;

[Keyless]
public partial class AnalyticsTrackingDbTrackingMessageCollection
{
    [StringLength(8000)]
    [Unicode(false)]
    public string Date { get; set; }

    [Column("_rid")]
    [StringLength(8000)]
    [Unicode(false)]
    public string Rid { get; set; }

    [Column("_etag")]
    [StringLength(8000)]
    [Unicode(false)]
    public string Etag { get; set; }

    [Column("_ts")]
    public long? Ts { get; set; }

    [Unicode(false)]
    public string Data { get; set; }

    [StringLength(8000)]
    [Unicode(false)]
    public string DocType { get; set; }

    [StringLength(8000)]
    [Unicode(false)]
    public string PartitionKey { get; set; }

    [Column("id")]
    [StringLength(8000)]
    [Unicode(false)]
    public string Id { get; set; }
}
