﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankingSystem.API.Models;
using BankingSystem.API.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using BankingSystem.API.DTO;

namespace RESTful_API__ASP.NET_Core.Repository
{
    public class KycRepository : IKycRepository
    {
        private readonly ApplicationDbContext _context;

        public KycRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<KycDocument> AddKycDocumentAsync(KycDocument kycDocument)
        {
            _context.KycDocument.Add(kycDocument);
            await _context.SaveChangesAsync();
            return kycDocument;
        }

        public async Task<KycDocument> UpdateKycDocumentAsync(int KYCId, KycDocument updatedKycDocument)
        {
            var existingKycDocument = await _context.KycDocument.FindAsync(KYCId);
            if (existingKycDocument != null)
            {
                existingKycDocument.FatherName = updatedKycDocument.FatherName;
                existingKycDocument.MotherName = updatedKycDocument.MotherName;
                existingKycDocument.GrandFatherName = updatedKycDocument.GrandFatherName;
                existingKycDocument.UserImagePath = updatedKycDocument.UserImagePath;
                existingKycDocument.CitizenshipImagePath = updatedKycDocument.CitizenshipImagePath;
                existingKycDocument.PermanentAddress = updatedKycDocument.PermanentAddress;
                existingKycDocument.UploadedAt = updatedKycDocument.UploadedAt;

                await _context.SaveChangesAsync();
                return existingKycDocument;
            }
            return null;
        }

        //no need for all KYC docs at once
        public async Task<IEnumerable<KycDocument>> GetKycDocumentAsync()
        {
            return await _context.KycDocument.ToListAsync();
        }

        public async Task<KycDocument?> GetKYCIdAsync(int KYCId)
        {
            return await _context.KycDocument.FindAsync(KYCId);
        }

        public async Task<KycDocument> GetKycByUserIdAsync(int userId)
        {
            return await _context.KycDocument.Where(k => k.UserId == userId).FirstOrDefaultAsync();
        }

        public void DeleteKycDocumentAsync(int KYCId)
        {
            var kycDocument = GetKYCIdAsync(KYCId);
            if (kycDocument != null)
            {
                _context.KycDocument.Remove(kycDocument.Result);
                 _context.SaveChangesAsync();
            }
        }

        public Task<KycDocument> UpdateKycDocumentAsync(int KYCId, JsonPatchDocument<KycDocumentDTO> kycDetails)
        {
            throw new NotImplementedException();
        }
    }
}
