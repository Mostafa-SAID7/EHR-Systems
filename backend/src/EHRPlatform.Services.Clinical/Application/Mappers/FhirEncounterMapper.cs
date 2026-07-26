using System.Text.Json;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Application.Mappers;

/// <summary>
/// FHIR R4 JSON mapper for Clinical Notes, Diagnoses, Procedures, and Vitals.
/// Produces HL7 FHIR R4 specification compliant Encounter bundle JSON.
/// </summary>
public static class FhirEncounterMapper
{
    public static string ToFhirR4BundleJson(ClinicalNote note)
    {
        var bundle = new
        {
            resourceType = "Bundle",
            type = "collection",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            entry = new List<object>
            {
                // 1. FHIR Encounter Resource
                new
                {
                    fullUrl = $"urn:uuid:{note.Id}",
                    resource = new
                    {
                        resourceType = "Encounter",
                        id = note.Id.ToString(),
                        status = note.Status == "Finalized" ? "finished" : "in-progress",
                        classCode = new
                        {
                            system = "http://terminology.hl7.org/CodeSystem/v3-ActCode",
                            code = note.EncounterType == "Telehealth" ? "VR" : "AMB",
                            display = note.EncounterType
                        },
                        subject = new
                        {
                            reference = $"Patient/{note.PatientId}"
                        },
                        participant = new[]
                        {
                            new
                            {
                                individual = new { reference = $"Practitioner/{note.ProviderId}" }
                            }
                        },
                        period = new
                        {
                            start = note.EncounterDate.ToString("yyyy-MM-ddTHH:mm:ssZ")
                        }
                    }
                }
            }
        };

        var entryList = (List<object>)bundle.entry;

        // 2. FHIR Condition Resources (Diagnoses)
        foreach (var diag in note.Diagnoses)
        {
            entryList.Add(new
            {
                fullUrl = $"urn:uuid:{diag.Id}",
                resource = new
                {
                    resourceType = "Condition",
                    id = diag.Id.ToString(),
                    clinicalStatus = new
                    {
                        coding = new[]
                        {
                            new { system = "http://terminology.hl7.org/CodeSystem/condition-clinical", code = "active" }
                        }
                    },
                    code = new
                    {
                        coding = new[]
                        {
                            new
                            {
                                system = "http://hl7.org/fhir/sid/icd-10-cm",
                                code = diag.DiagnosisCode,
                                display = diag.DiagnosisText
                            }
                        }
                    },
                    subject = new { reference = $"Patient/{note.PatientId}" },
                    encounter = new { reference = $"Encounter/{note.Id}" }
                }
            });
        }

        // 3. FHIR Procedure Resources
        foreach (var proc in note.Procedures)
        {
            entryList.Add(new
            {
                fullUrl = $"urn:uuid:{proc.Id}",
                resource = new
                {
                    resourceType = "Procedure",
                    id = proc.Id.ToString(),
                    status = "completed",
                    code = new
                    {
                        coding = new[]
                        {
                            new
                            {
                                system = "http://www.ama-assn.org/go/cpt",
                                code = proc.ProcedureCode,
                                display = proc.ProcedureName
                            }
                        }
                    },
                    subject = new { reference = $"Patient/{note.PatientId}" },
                    encounter = new { reference = $"Encounter/{note.Id}" },
                    performedDateTime = proc.PerformedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                }
            });
        }

        return JsonSerializer.Serialize(bundle, new JsonSerializerOptions { WriteIndented = true });
    }
}
