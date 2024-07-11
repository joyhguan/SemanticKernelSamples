using System.ComponentModel;

using Microsoft.SemanticKernel;

namespace SemanticKernelSamples.Samples.Planning;

internal class DocumentPlugins
{
    private record Document(int Id, int CreatedById, DateTime CreatedDateTimeUtc, string Status);

    [KernelFunction, Description("Get recent modified document IDs")]
    public Task<IReadOnlyList<int>> GetRecentModifiedDocumentIds()
    {
        return Task.FromResult<IReadOnlyList<int>>(new[] { 1, 2, 3 });
    }

    [KernelFunction, Description("Get document details by document ID. Returns an object containing the document ID, created by, created date, and status.")]
    public Task<object> GetDocumentDetails(int id)
    {
        var document = id switch
        {
            1 => new Document(101, 7891, new DateTime(2023, 12, 1, 3, 0, 1, DateTimeKind.Utc), "Active"),
            2 => new Document(102, 7892, new DateTime(2024, 5, 15, 11, 12, 0, DateTimeKind.Utc), "Inactive"),
            3 => new Document(103, 7891, new DateTime(2023, 12, 1, 1, 0, 1, DateTimeKind.Utc), "Archived"),
            _ => throw new Exception("Document not found"),
        };

        return Task.FromResult<object>(document);
    }

    [KernelFunction, Description("Get custom field names for a document by document ID. Returns a list of field names.")]
    public Task<IReadOnlyList<string>> GetCustomFieldNames(int documentId)
    {
        List<string> fieldNames = documentId switch
        {
            1 => ["Title", "Author", "Purpose", "Scope of Work", "Background", "Context"],
            2 => ["ProjectName", "Budget", "Purpose", "Scope of Work", "Background", "Context"],
            3 => ["ReportTitle", "Findings", "Recommendations", "Purpose", "Scope of Work", "Background", "Context"],
            _ => throw new Exception("Document not found"),
        };

        return Task.FromResult<IReadOnlyList<string>>(fieldNames);
    }

    [KernelFunction, Description("Get custom field values for a document by document ID and field name. Returns a dictionary of field names and their values.")]
    public Task<object> GetCustomFieldValues(int documentId, string fieldName)
    {
        var fieldValues = documentId switch
        {
            1 => new Dictionary<string, object>
        {
            { "Title", "Introduction to Machine Learning" },
            { "Author", "John Doe" },
            { "Purpose", "To educate readers about the basics of machine learning." },
            { "Scope of Work", "Covers fundamental algorithms, techniques, and real-world applications." },
            { "Background", "Machine learning is a rapidly growing field in artificial intelligence." },
            { "Context", "Suitable for beginners and professionals looking to refresh their knowledge." }
        },
            2 => new Dictionary<string, object>
        {
            { "ProjectName", "AI Development Project" },
            { "Budget", 150000 },
            { "Purpose", "To develop advanced AI capabilities for client solutions." },
            { "Scope of Work", "Includes design, development, testing, and deployment of AI systems." },
            { "Background", "Client requires advanced AI solutions to stay competitive." },
            { "Context", "Project spans multiple departments and requires coordinated efforts." }
        },
            3 => new Dictionary<string, object>
        {
            { "ReportTitle", "Market Analysis Q1 2023" },
            { "Findings", "Increased market share in tech sector." },
            { "Recommendations", "Invest in emerging technologies." },
            { "Purpose", "To provide insights on market trends and company performance." },
            { "Scope of Work", "Analyzes market data, competitor performance, and economic indicators." },
            { "Background", "Quarterly market analysis to inform strategic decisions." },
            { "Context", "Report is used by senior management for planning and investment." }
        },
            _ => throw new Exception("Document not found"),
        };

        return Task.FromResult(fieldValues[fieldName]);
    }

    [KernelFunction, Description("Search for document names and return the corresponding document IDs.")]
    public Task<IReadOnlyList<int>> SearchDocumentNames(string searchQuery)
    {
        var documents = new List<(int DocumentID, string DocumentName)>
    {
        (1, "Privacy Agreement"),
        (2, "Service Agreement"),
        (3, "Non-Disclosure Agreement (NDA)")
    };

        var matchingDocumentIds = documents
            .Where(doc => doc.DocumentName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            .Select(doc => doc.DocumentID)
            .ToList();

        return Task.FromResult<IReadOnlyList<int>>(matchingDocumentIds);
    }

    [KernelFunction, Description("Get document contents by document ID. Returns the full content of the document as a string.")]
    public Task<string> GetDocumentContents(int documentId)
    {
        var content = documentId switch
        {
            1 =>
            "Privacy Agreement\n" +
            "=================\n\n" +
            "1. Purpose\n" +
            "-----------\n" +
            "This Privacy Agreement outlines our commitment to protecting the privacy of our users. It details how we collect, use, and safeguard personal information.\n\n" +
            "2. Background\n" +
            "-------------\n" +
            "Our company is dedicated to maintaining the confidentiality of personal data. This agreement is in accordance with applicable data protection laws and regulations.\n\n" +
            "3. Scope of Work\n" +
            "----------------\n" +
            "This agreement applies to all personal data collected through our services, including names, addresses, email addresses, and payment information. It covers how data is processed, stored, and protected.\n\n" +
            "4. Context\n" +
            "----------\n" +
            "This Privacy Agreement is relevant to all users of our services, including website visitors, customers, and business partners.\n\n" +
            "5. Data Collection\n" +
            "------------------\n" +
            "We collect personal information directly from users through forms, surveys, and account registrations. We may also collect data through automated means, such as cookies and web beacons.\n\n" +
            "6. Data Use\n" +
            "-----------\n" +
            "Personal data is used to provide and improve our services, communicate with users, process transactions, and comply with legal obligations.\n\n" +
            "7. Data Protection\n" +
            "------------------\n" +
            "We implement a variety of security measures to ensure the protection of personal data. Access to personal information is restricted to authorized personnel only.\n\n" +
            "8. User Rights\n" +
            "--------------\n" +
            "Users have the right to access, correct, or delete their personal information. They can also object to or restrict certain data processing activities.\n\n" +
            "9. Changes to This Agreement\n" +
            "----------------------------\n" +
            "We may update this Privacy Agreement periodically. Users will be notified of any significant changes through our website or direct communication.\n\n" +
            "10. Contact Information\n" +
            "-----------------------\n" +
            "For questions or concerns about this Privacy Agreement, please contact our Legal Team at legal@example.com.",

            2 =>
            "Service Agreement\n" +
            "=================\n\n" +
            "1. Purpose\n" +
            "-----------\n" +
            "This Service Agreement sets forth the terms and conditions under which we will provide services to the client.\n\n" +
            "2. Background\n" +
            "-------------\n" +
            "This agreement is designed to formalize the relationship between our company and the client, ensuring a clear understanding of the roles and responsibilities of each party.\n\n" +
            "3. Scope of Work\n" +
            "----------------\n" +
            "The services to be provided under this agreement include design, development, testing, and deployment of software solutions. Detailed specifications are outlined in the attached project plan.\n\n" +
            "4. Context\n" +
            "----------\n" +
            "This agreement is relevant to all projects undertaken by our company for the client and includes specific terms for deliverables, timelines, and payment schedules.\n\n" +
            "5. Deliverables\n" +
            "--------------\n" +
            "The key deliverables under this agreement include completed software applications, user documentation, and training materials.\n\n" +
            "6. Timelines\n" +
            "------------\n" +
            "Project milestones and deadlines are outlined in the project plan. Any changes to the schedule must be agreed upon by both parties in writing.\n\n" +
            "7. Payment Terms\n" +
            "---------------\n" +
            "The total project cost is $150,000, payable in installments as follows: $50,000 upon signing, $50,000 upon completion of development, and $50,000 upon final delivery.\n\n" +
            "8. Confidentiality\n" +
            "------------------\n" +
            "Both parties agree to maintain the confidentiality of proprietary information disclosed during the course of the project.\n\n" +
            "9. Liability\n" +
            "------------\n" +
            "Our company’s liability for any damages arising from this agreement is limited to the total amount paid by the client.\n\n" +
            "10. Termination\n" +
            "---------------\n" +
            "This agreement may be terminated by either party with 30 days' written notice. In the event of termination, the client will pay for all services rendered up to the termination date.",

            3 =>
            "Non-Disclosure Agreement (NDA)\n" +
            "=============================\n\n" +
            "1. Purpose\n" +
            "-----------\n" +
            "This Non-Disclosure Agreement (NDA) is intended to protect confidential information exchanged between the parties.\n\n" +
            "2. Background\n" +
            "-------------\n" +
            "The parties wish to explore a potential business relationship, during which they may disclose confidential information to each other.\n\n" +
            "3. Scope of Work\n" +
            "----------------\n" +
            "This agreement covers all confidential information, including business plans, financial data, technical information, and trade secrets, disclosed by one party to the other.\n\n" +
            "4. Context\n" +
            "----------\n" +
            "This NDA is applicable to all discussions and documents exchanged between the parties during their collaboration.\n\n" +
            "5. Definition of Confidential Information\n" +
            "----------------------------------------\n" +
            "Confidential information includes all non-public information, whether written or oral, that is designated as confidential or that reasonably should be understood to be confidential.\n\n" +
            "6. Obligations of Receiving Party\n" +
            "---------------------------------\n" +
            "The receiving party agrees to use the confidential information solely for the purpose of evaluating the potential business relationship and to take reasonable measures to protect its confidentiality.\n\n" +
            "7. Exclusions from Confidential Information\n" +
            "-------------------------------------------\n" +
            "Confidential information does not include information that is already known to the receiving party, becomes publicly available through no fault of the receiving party, or is independently developed by the receiving party.\n\n" +
            "8. Duration\n" +
            "-----------\n" +
            "This NDA is effective as of the date signed by both parties and remains in effect for three years from the date of disclosure of the confidential information.\n\n" +
            "9. Governing Law\n" +
            "----------------\n" +
            "This agreement is governed by the laws of the jurisdiction in which the disclosing party is located.\n\n" +
            "10. Signatures\n" +
            "-------------\n" +
            "This NDA is executed by the duly authorized representatives of the parties as of the dates set forth below.\n\n" +
            "Disclosing Party: _______________________\n" +
            "Receiving Party: _______________________\n" +
            "Date: _________________________________",

            _ => throw new Exception("Document not found"),
        };

        return Task.FromResult(content);
    }
}